using WebAPI.DTO;

namespace WebAPI.Data.abstracts
{
    public interface ICartQueryRepository
    {
        Task<CartResponseDto> GetCartByUserIdAsync(int userId);
    }
}
