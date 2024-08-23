using AOP.Aspects;
using WebAPI.Data.abstracts;
using WebAPI.DTO;
using WebAPI.Services.abstracts;

namespace WebAPI.Services.concretes
{
    public class ReviewQueryService : IReviewQueryService
    {
        private readonly IReviewQueryRepository _reviewQueryRepository;

        public ReviewQueryService(IReviewQueryRepository reviewQueryRepository)
        {
            _reviewQueryRepository = reviewQueryRepository;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "review:")]
        public async Task<ReviewResponseDto> GetReviewByIdAsync(int reviewId)
        {
            return await _reviewQueryRepository.GetByIdAsync(reviewId);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "reviews:product:")]
        public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByProductIdAsync(int productId)
        {
            return await _reviewQueryRepository.GetByProductIdAsync(productId);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "reviews:user:")]
        public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByUserIdAsync(int userId)
        {
            return await _reviewQueryRepository.GetByUserIdAsync(userId);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "reviews:average:")]
        public async Task<double> GetAverageRatingForProductAsync(int productId)
        {
            return await _reviewQueryRepository.GetAverageRatingByProductIdAsync(productId);
        }
    }
}
