using DataAccess.DTO;

namespace DataAccess.Repositories.abstracts
{
    public interface IOrderQueryRepository
    {
        Task<OrderResponseDto> GetByIdAsync(int id);
        Task<IEnumerable<OrderResponseDto>> GetAllAsync();
        Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(int userId);
    }
}
