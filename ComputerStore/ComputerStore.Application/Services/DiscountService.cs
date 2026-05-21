using ComputerStore.Application.DTOs;
using ComputerStore.Application.Interfaces;

namespace ComputerStore.Application.Services;

public class DiscountService : IDiscountService
{
    private const decimal CategoryDiscountRate = 0.05m;
    private readonly IProductRepository _productRepository;

    public DiscountService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<DiscountResultDto> CalculateAsync(IEnumerable<BasketItemDto> items)
    {
        var basketList = items.ToList();

        if (basketList.Count == 0)
            throw new ArgumentException("Basket is empty.");

        // Load products and validate stock
        var lines = new List<(Domain.Entities.Product Product, int Quantity)>();

        foreach (var item in basketList)
        {
            if (item.Quantity <= 0)
                throw new ArgumentException($"Quantity for product {item.ProductId} must be greater than zero.");

            var product = await _productRepository.GetByIdAsync(item.ProductId)
                ?? throw new KeyNotFoundException($"Product with id {item.ProductId} not found.");

            if (product.Stock < item.Quantity)
                throw new InvalidOperationException(
                    $"Not enough stock for '{product.Name}'. Available: {product.Stock}, requested: {item.Quantity}.");

            lines.Add((product, item.Quantity));
        }

        // Determine which category names appear more than once across the basket
        var categoryNameCounts = lines
            .SelectMany(l => l.Product.Categories.Select(c => c.Name))
            .GroupBy(n => n)
            .ToDictionary(g => g.Key, g => g.Count());

        var result = new DiscountResultDto();

        foreach (var (product, quantity) in lines)
        {
            var productCategoryNames = product.Categories
                .Select(c => c.Name)
                .ToHashSet();

            bool qualifiesForDiscount = productCategoryNames
                .Any(cn => categoryNameCounts.TryGetValue(cn, out var count) && count > 1);

            decimal discountPercent = qualifiesForDiscount ? CategoryDiscountRate * 100 : 0m;

            decimal lineOriginal = product.Price * quantity;
            decimal lineDiscount = qualifiesForDiscount ? lineOriginal * CategoryDiscountRate : 0m;
            decimal lineTotal = lineOriginal - lineDiscount;

            result.Lines.Add(new DiscountLineDto
            {
                ProductName = product.Name,
                Quantity = quantity,
                UnitPrice = product.Price,
                DiscountPercent = discountPercent,
                LineTotal = lineTotal
            });

            result.OriginalTotal += lineOriginal;
            result.DiscountAmount += lineDiscount;
        }

        result.FinalTotal = result.OriginalTotal - result.DiscountAmount;
        return result;
    }
}
