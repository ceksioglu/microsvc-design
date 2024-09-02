using AOP.Aspects;
using DataAccess.DTO;
using DataAccess.Repositories.abstracts;
using EventHandler.Events.ReviewEvents;
using EventHandler.Handlers.abstracts;
using Services.Services.abstracts;

namespace Services.Services.concretes
{
    public class ReviewCommandService : IReviewCommandService
    {
        private readonly IReviewCommandRepository _reviewCommandRepository;
        private readonly IEventPublisher _eventPublisher;

        public ReviewCommandService(IReviewCommandRepository reviewCommandRepository, IEventPublisher eventPublisher)
        {
            _reviewCommandRepository = reviewCommandRepository;
            _eventPublisher = eventPublisher;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<ReviewResponseDto> CreateReviewAsync(ReviewCreateDto reviewDto)
        {
            var result = await _reviewCommandRepository.CreateAsync(reviewDto);
            await _eventPublisher.PublishAsync(new ReviewCreatedEvent
            {
                ReviewId = result.Id,
                UserId = result.UserId,
                ProductId = result.ProductId,
                Rating = result.Rating
            }, "review_events", "review_created");
            return result;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<ReviewResponseDto> UpdateReviewAsync(int reviewId, ReviewUpdateDto reviewDto)
        {
            var result = await _reviewCommandRepository.UpdateAsync(reviewId, reviewDto);
            await _eventPublisher.PublishAsync(new ReviewUpdatedEvent
            {
                ReviewId = result.Id,
                Rating = result.Rating
            }, "review_events", "review_updated");
            return result;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            var result = await _reviewCommandRepository.DeleteAsync(reviewId);
            if (result)
            {
                await _eventPublisher.PublishAsync(new ReviewDeletedEvent
                {
                    ReviewId = reviewId
                }, "review_events", "review_deleted");
            }
            return result;
        }
    }
}