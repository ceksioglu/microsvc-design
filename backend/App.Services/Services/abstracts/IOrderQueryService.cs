using DataAccess.DTO;

namespace Services.Services.abstracts
{
    public interface IOrderQueryService
    {
        Task<OrderResponseDto> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(int userId);
        Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync();
    }
}
