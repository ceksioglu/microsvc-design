using System.Threading.Tasks;
using WebAPI.DTO;

namespace WebAPI.Data.abstracts
{
    public interface IOrderCommandRepository
    {
        Task<OrderResponseDto> CreateAsync(OrderCreateDto orderDto);
        Task<OrderResponseDto> UpdateAsync(int id, OrderUpdateDto orderDto);
        Task<bool> DeleteAsync(int id);
    }
}
