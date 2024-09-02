using DataAccess.DTO;

namespace Services.Services.abstracts
{
    public interface ICartQueryService
    {
        Task<CartResponseDto> GetCartByUserIdAsync(int userId);
    }
}
