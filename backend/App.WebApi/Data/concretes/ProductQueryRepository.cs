using AOP.Aspects;
using AutoMapper;
using Core.Redis.abstracts;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data.abstracts;
using WebAPI.DTO;

namespace WebAPI.Data.concretes
{
    public class ProductQueryRepository : IProductQueryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IRedisService _redisService;

        public ProductQueryRepository(
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
        [CachingAspect(300, "product:")]
        public async Task<ProductResponseDto> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            return product != null ? _mapper.Map<ProductResponseDto>(product) : null;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "products:all")]
        public async Task<IEnumerable<ProductListItemDto>> GetAllAsync()
        {
            var products = await _context.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProductListItemDto>>(products);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<IEnumerable<ProductListItemDto>> SearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                throw new ArgumentException("Search term cannot be empty.", nameof(term));

            var products = await _context.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted &&
                            (p.Name.Contains(term) || p.Description.Contains(term)))
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProductListItemDto>>(products);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "products:category:")]
        public async Task<IEnumerable<ProductListItemDto>> GetProductsByCategoryAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be empty.", nameof(category));

            var products = await _context.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.Category == category)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProductListItemDto>>(products);
        }
    }
}
