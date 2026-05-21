using ComputerStore.Application.DTOs;

namespace ComputerStore.Application.Interfaces;

public interface IDiscountService
{
    Task<DiscountResultDto> CalculateAsync(IEnumerable<BasketItemDto> items);
}
