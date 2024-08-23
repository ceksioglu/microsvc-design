using WebAPI.DTO;

namespace WebAPI.Services.abstracts
{
    public interface IReviewQueryService
    {
        Task<ReviewResponseDto> GetReviewByIdAsync(int reviewId);
        Task<IEnumerable<ReviewResponseDto>> GetReviewsByProductIdAsync(int productId);
        Task<IEnumerable<ReviewResponseDto>> GetReviewsByUserIdAsync(int userId);
        Task<double> GetAverageRatingForProductAsync(int productId);
    }
}
