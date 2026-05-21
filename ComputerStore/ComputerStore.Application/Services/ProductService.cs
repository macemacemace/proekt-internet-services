using AutoMapper;
using ComputerStore.Application.DTOs;
using ComputerStore.Application.Interfaces;
using ComputerStore.Domain.Entities;

namespace ComputerStore.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product with id {id} not found.");
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        ValidateProductDto(dto);

        var product = _mapper.Map<Product>(dto);
        product.Categories = await ResolveCategories(dto.Categories);

        var created = await _productRepository.CreateAsync(product);
        return _mapper.Map<ProductDto>(created);
    }

    public async Task<ProductDto> UpdateAsync(int id, CreateProductDto dto)
    {
        ValidateProductDto(dto);

        var product = await _productRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product with id {id} not found.");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Categories = await ResolveCategories(dto.Categories);

        var updated = await _productRepository.UpdateAsync(product);
        return _mapper.Map<ProductDto>(updated);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product with id {id} not found.");
        await _productRepository.DeleteAsync(product);
    }

    public async Task ImportStockAsync(IEnumerable<StockImportItemDto> items)
    {
        foreach (var item in items)
        {
            var product = await _productRepository.GetByNameAsync(item.Name);

            if (product == null)
            {
                product = new Product
                {
                    Name = item.Name,
                    Price = item.Price,
                    Stock = item.Quantity,
                    Categories = await ResolveCategories(item.Categories)
                };
                await _productRepository.CreateAsync(product);
            }
            else
            {
                product.Stock += item.Quantity;
                product.Price = item.Price;
                await _productRepository.UpdateAsync(product);
            }
        }
    }

    private static void ValidateProductDto(CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Product name is required.");
        if (dto.Price <= 0)
            throw new ArgumentException("Product price must be greater than zero.");
        if (dto.Categories == null || dto.Categories.Count == 0)
            throw new ArgumentException("At least one category is required.");
    }

    private async Task<ICollection<Category>> ResolveCategories(List<string> categoryNames)
    {
        var categories = new List<Category>();

        foreach (var name in categoryNames)
        {
            var trimmed = name.Trim();
            var category = await _categoryRepository.GetByNameAsync(trimmed);

            if (category == null)
            {
                category = await _categoryRepository.CreateAsync(new Category { Name = trimmed });
            }

            categories.Add(category);
        }

        return categories;
    }
}
