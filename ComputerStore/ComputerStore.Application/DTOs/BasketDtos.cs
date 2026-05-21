namespace ComputerStore.Application.DTOs;

public class BasketItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class DiscountResultDto
{
    public decimal OriginalTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalTotal { get; set; }
    public List<DiscountLineDto> Lines { get; set; } = new();
}

public class DiscountLineDto
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal LineTotal { get; set; }
}
