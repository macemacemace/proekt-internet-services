using ComputerStore.Application.DTOs;
using ComputerStore.Application.Interfaces;
using ComputerStore.Application.Services;
using ComputerStore.Domain.Entities;
using Moq;

namespace ComputerStore.Tests.Unit;

public class DiscountServiceTests
{
    private readonly Mock<IProductRepository> _repoMock = new();
    private readonly DiscountService _service;

    public DiscountServiceTests()
    {
        _service = new DiscountService(_repoMock.Object);
    }

    private static Product MakeProduct(int id, string name, decimal price, int stock, params string[] categories)
    {
        var product = new Product { Id = id, Name = name, Price = price, Stock = stock };
        product.Categories = categories.Select((c, i) => new Category { Id = i + 1, Name = c }).ToList();
        return product;
    }

    [Fact]
    public async Task Calculate_SingleProduct_NoDiscount()
    {
        var cpu = MakeProduct(1, "i9-9900K", 475.99m, 10, "CPU");
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cpu);

        var result = await _service.CalculateAsync(new[] { new BasketItemDto { ProductId = 1, Quantity = 1 } });

        Assert.Equal(0, result.DiscountAmount);
        Assert.Equal(475.99m, result.FinalTotal);
    }

    [Fact]
    public async Task Calculate_TwoProductsSameCategory_AppliesDiscount()
    {
        var cpu1 = MakeProduct(1, "i9-9900K", 475.99m, 10, "CPU");
        var cpu2 = MakeProduct(2, "Ryzen 9", 400.00m, 10, "CPU");
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cpu1);
        _repoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(cpu2);

        var result = await _service.CalculateAsync(new[]
        {
            new BasketItemDto { ProductId = 1, Quantity = 1 },
            new BasketItemDto { ProductId = 2, Quantity = 1 }
        });

        Assert.True(result.DiscountAmount > 0);
        Assert.All(result.Lines, l => Assert.Equal(5, l.DiscountPercent));
    }

    [Fact]
    public async Task Calculate_DifferentCategories_NoDiscount()
    {
        var cpu = MakeProduct(1, "i9-9900K", 475.99m, 10, "CPU");
        var keyboard = MakeProduct(2, "Razer KB", 89.99m, 10, "Keyboard");
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cpu);
        _repoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(keyboard);

        var result = await _service.CalculateAsync(new[]
        {
            new BasketItemDto { ProductId = 1, Quantity = 1 },
            new BasketItemDto { ProductId = 2, Quantity = 1 }
        });

        Assert.Equal(0, result.DiscountAmount);
    }

    [Fact]
    public async Task Calculate_InsufficientStock_ThrowsInvalidOperationException()
    {
        var cpu = MakeProduct(1, "i9-9900K", 475.99m, 1, "CPU");
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cpu);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CalculateAsync(new[] { new BasketItemDto { ProductId = 1, Quantity = 5 } }));

        Assert.Contains("stock", ex.Message);
    }

    [Fact]
    public async Task Calculate_ProductNotFound_ThrowsKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.CalculateAsync(new[] { new BasketItemDto { ProductId = 99, Quantity = 1 } }));
    }

    [Fact]
    public async Task Calculate_EmptyBasket_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CalculateAsync(Array.Empty<BasketItemDto>()));
    }

    [Fact]
    public async Task Calculate_TwoCpusOneCpu_BothUnitsDiscounted()
    {
        var cpu1 = MakeProduct(1, "i9-9900K", 475.99m, 10, "CPU");
        var cpu2 = MakeProduct(2, "Ryzen 9", 400.00m, 10, "CPU");
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cpu1);
        _repoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(cpu2);

        var result = await _service.CalculateAsync(new[]
        {
            new BasketItemDto { ProductId = 1, Quantity = 2 },
            new BasketItemDto { ProductId = 2, Quantity = 1 }
        });

        Assert.All(result.Lines, l => Assert.Equal(5, l.DiscountPercent));
        Assert.True(Math.Abs(result.FinalTotal - 1284.38m) <= 0.01m);
    }
}
