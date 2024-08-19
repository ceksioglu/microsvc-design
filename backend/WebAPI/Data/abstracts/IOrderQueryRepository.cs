using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.DTO;

namespace WebAPI.Data.abstracts
{
    public interface IOrderQueryRepository
    {
        Task<OrderResponseDto> GetByIdAsync(int id);
        Task<IEnumerable<OrderResponseDto>> GetAllAsync();
        Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(int userId);
    }
}
