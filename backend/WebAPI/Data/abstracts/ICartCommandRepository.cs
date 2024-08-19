using System.Threading.Tasks;
using WebAPI.DTO;

namespace WebAPI.Data.abstracts
{
    public interface ICartCommandRepository
    {
        Task<CartResponseDto> AddItemAsync(int userId, CartItemCreateDto itemDto);
        Task<CartResponseDto> UpdateItemAsync(int userId, int productId, CartItemUpdateDto itemDto);
        Task<bool> RemoveItemAsync(int userId, int productId);
        Task<bool> ClearCartAsync(int userId);
    }
}
