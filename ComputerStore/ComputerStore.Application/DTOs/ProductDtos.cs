namespace ComputerStore.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public List<string> Categories { get; set; } = new();
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public List<string> Categories { get; set; } = new();
}

public class StockImportItemDto
{
    public string Name { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = new();
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
