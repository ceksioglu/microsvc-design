using DataAccess.DTO;

namespace Services.Services.abstracts
{
    public interface ICartCommandService
    {
        Task<CartResponseDto> AddItemToCartAsync(int userId, CartItemCreateDto itemDto);
        Task<CartResponseDto> UpdateCartItemAsync(int userId, int productId, CartItemUpdateDto itemDto);
        Task<bool> RemoveItemFromCartAsync(int userId, int productId);
        Task<bool> ClearCartAsync(int userId);
    }
}
