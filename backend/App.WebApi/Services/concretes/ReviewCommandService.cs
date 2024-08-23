using AOP.Aspects;
using WebAPI.Data.abstracts;
using WebAPI.DTO;
using WebAPI.Services.abstracts;

namespace WebAPI.Services.concretes
{
    public class ReviewCommandService : IReviewCommandService
    {
        private readonly IReviewCommandRepository _reviewCommandRepository;

        public ReviewCommandService(IReviewCommandRepository reviewCommandRepository)
        {
            _reviewCommandRepository = reviewCommandRepository;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<ReviewResponseDto> CreateReviewAsync(ReviewCreateDto reviewDto)
        {
            return await _reviewCommandRepository.CreateAsync(reviewDto);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<ReviewResponseDto> UpdateReviewAsync(int reviewId, ReviewUpdateDto reviewDto)
        {
            return await _reviewCommandRepository.UpdateAsync(reviewId, reviewDto);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            return await _reviewCommandRepository.DeleteAsync(reviewId);
        }
    }
}
