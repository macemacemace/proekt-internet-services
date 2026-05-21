using ComputerStore.Application.DTOs;

namespace ComputerStore.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto> UpdateAsync(int id, CreateProductDto dto);
    Task DeleteAsync(int id);
    Task ImportStockAsync(IEnumerable<StockImportItemDto> items);
}
