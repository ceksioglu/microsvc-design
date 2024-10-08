using AOP.Aspects;
using AutoMapper;
using Core.Redis.abstracts;
using DataAccess.DTO;
using DataAccess.Repositories.abstracts;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.concretes
{
    public class ReviewQueryRepository : IReviewQueryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IRedisService _redisService;

        public ReviewQueryRepository(
            ApplicationDbContext context,
            IMapper mapper,
            IRedisService redisService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _redisService = redisService ?? throw new ArgumentNullException(nameof(redisService));
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "review:")]
        public async Task<ReviewResponseDto> GetByIdAsync(int id)
        {
            var review = await _context.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            return review != null ? _mapper.Map<ReviewResponseDto>(review) : null;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "reviews:product:")]
        public async Task<IEnumerable<ReviewResponseDto>> GetByProductIdAsync(int productId)
        {
            var reviews = await _context.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ReviewResponseDto>>(reviews);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "reviews:user:")]
        public async Task<IEnumerable<ReviewResponseDto>> GetByUserIdAsync(int userId)
        {
            var reviews = await _context.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ReviewResponseDto>>(reviews);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "reviews:average:")]
        public async Task<double> GetAverageRatingByProductIdAsync(int productId)
        {
            return await _context.Reviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId)
                .AverageAsync(r => r.Rating);
        }
    }
}
