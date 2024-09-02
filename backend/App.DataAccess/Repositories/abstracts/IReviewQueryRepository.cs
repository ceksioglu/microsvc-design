using DataAccess.DTO;

namespace DataAccess.Repositories.abstracts
{
    public interface IReviewQueryRepository
    {
        Task<ReviewResponseDto> GetByIdAsync(int id);
        Task<IEnumerable<ReviewResponseDto>> GetByProductIdAsync(int productId);
        Task<IEnumerable<ReviewResponseDto>> GetByUserIdAsync(int userId);
        Task<double> GetAverageRatingByProductIdAsync(int productId);
    }
}
