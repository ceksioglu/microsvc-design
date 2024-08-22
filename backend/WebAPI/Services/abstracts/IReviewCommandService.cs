using WebAPI.DTO;

namespace WebAPI.Services.abstracts
{
    public interface IReviewCommandService
    {
        Task<ReviewResponseDto> CreateReviewAsync(ReviewCreateDto reviewDto);
        Task<ReviewResponseDto> UpdateReviewAsync(int reviewId, ReviewUpdateDto reviewDto);
        Task<bool> DeleteReviewAsync(int reviewId);
    }
}
