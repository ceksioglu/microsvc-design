using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data.abstracts;
using WebAPI.DTO;
using WebAPI.Models;
using WebAPI.Aspects;
using WebAPI.Packages.RabbitMQ.abstracts;

namespace WebAPI.Data.concretes
{
    public class ReviewCommandRepository : IReviewCommandRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IRabbitMQService _rabbitMQService;

        public ReviewCommandRepository(
            ApplicationDbContext context,
            IMapper mapper,
            IRabbitMQService rabbitMQService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _rabbitMQService = rabbitMQService ?? throw new ArgumentNullException(nameof(rabbitMQService));
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<ReviewResponseDto> CreateAsync(ReviewCreateDto reviewDto)
        {
            if (reviewDto == null)
                throw new ArgumentNullException(nameof(reviewDto));

            var review = _mapper.Map<Review>(reviewDto);
            review.CreatedAt = DateTime.UtcNow;

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            await PublishReviewEvent("review_created", review);

            return _mapper.Map<ReviewResponseDto>(review);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<ReviewResponseDto> UpdateAsync(int id, ReviewUpdateDto reviewDto)
        {
            if (reviewDto == null)
                throw new ArgumentNullException(nameof(reviewDto));

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                throw new KeyNotFoundException($"Review with id {id} not found.");

            _mapper.Map(reviewDto, review);

            await _context.SaveChangesAsync();

            await PublishReviewEvent("review_updated", review);

            return _mapper.Map<ReviewResponseDto>(review);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<bool> DeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return false;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            await PublishReviewEvent("review_deleted", id);

            return true;
        }

        [LoggingAspect]
        [ExceptionAspect]
        private async Task PublishReviewEvent(string eventType, object payload)
        {
            await _rabbitMQService.PublishMessage("review_events", eventType, payload);
        }
    }
}
