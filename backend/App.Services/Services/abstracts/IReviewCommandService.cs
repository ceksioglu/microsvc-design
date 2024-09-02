using DataAccess.DTO;

namespace Services.Services.abstracts
{
    public interface IReviewCommandService
    {
        Task<ReviewResponseDto> CreateReviewAsync(ReviewCreateDto reviewDto);
        Task<ReviewResponseDto> UpdateReviewAsync(int reviewId, ReviewUpdateDto reviewDto);
        Task<bool> DeleteReviewAsync(int reviewId);
    }
}
