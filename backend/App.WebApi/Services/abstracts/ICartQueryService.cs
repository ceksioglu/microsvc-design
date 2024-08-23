using WebAPI.DTO;

namespace WebAPI.Services.abstracts
{
    public interface ICartQueryService
    {
        Task<CartResponseDto> GetCartByUserIdAsync(int userId);
    }
}
