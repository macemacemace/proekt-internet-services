using System.Net;
using System.Net.Http.Json;
using ComputerStore.Application.DTOs;

namespace ComputerStore.Tests.Integration;

public class DiscountIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DiscountIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<ProductDto> CreateProductAsync(string name, decimal price, int stock, List<string> categories)
    {
        var items = new List<StockImportItemDto>
        {
            new() { Name = name, Price = price, Quantity = stock, Categories = categories }
        };
        await _client.PostAsJsonAsync("/api/products/import", items);

        var all = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products");
        return all!.First(p => p.Name == name);
    }

    [Fact]
    public async Task Calculate_SharedCategory_AppliesDiscount()
    {
        var cpu1 = await CreateProductAsync("CalcCPU1", 100m, 10, new() { "CalcCPU" });
        var cpu2 = await CreateProductAsync("CalcCPU2", 200m, 10, new() { "CalcCPU" });

        var basket = new List<BasketItemDto>
        {
            new() { ProductId = cpu1.Id, Quantity = 1 },
            new() { ProductId = cpu2.Id, Quantity = 1 }
        };

        var response = await _client.PostAsJsonAsync("/api/discount/calculate", basket);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DiscountResultDto>();
        Assert.True(result!.DiscountAmount > 0);
    }

    [Fact]
    public async Task Calculate_InsufficientStock_Returns422()
    {
        var product = await CreateProductAsync("LowStock", 50m, 1, new() { "SomeCategory" });

        var basket = new List<BasketItemDto>
        {
            new() { ProductId = product.Id, Quantity = 100 }
        };

        var response = await _client.PostAsJsonAsync("/api/discount/calculate", basket);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
