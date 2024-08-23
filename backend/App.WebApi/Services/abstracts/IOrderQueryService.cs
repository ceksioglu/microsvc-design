using WebAPI.DTO;

namespace WebAPI.Services.abstracts
{
    public interface IOrderQueryService
    {
        Task<OrderResponseDto> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(int userId);
        Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync();
    }
}
