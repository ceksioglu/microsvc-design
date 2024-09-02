using DataAccess.DTO;

namespace DataAccess.Repositories.abstracts
{
    public interface ICartQueryRepository
    {
        Task<CartResponseDto> GetCartByUserIdAsync(int userId);
    }
}
