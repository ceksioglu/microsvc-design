using WebAPI.DTO;

namespace WebAPI.Services.abstracts
{
    public interface IOrderCommandService
    {
        Task<OrderResponseDto> CreateOrderAsync(OrderCreateDto orderDto);
        Task<OrderResponseDto> UpdateOrderStatusAsync(int orderId, string newStatus);
        Task<bool> CancelOrderAsync(int orderId);
    }
}
