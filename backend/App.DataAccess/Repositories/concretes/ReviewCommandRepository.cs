using AOP.Aspects;
using AutoMapper;
using DataAccess.DTO;
using DataAccess.Models;
using DataAccess.Repositories.abstracts;

namespace DataAccess.Repositories.concretes
{
    public class ReviewCommandRepository : IReviewCommandRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ReviewCommandRepository(
            ApplicationDbContext context,
            IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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
            review.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

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

            return true;
        }
    }
}