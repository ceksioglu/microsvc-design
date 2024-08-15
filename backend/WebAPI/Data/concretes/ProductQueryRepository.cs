using WebAPI.Models;
using WebAPI.Data.abstracts;
using WebAPI.Aspects;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Data.concretes
{
    public class ProductQueryRepository : IProductQueryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerFactory _loggerFactory;

        public ProductQueryRepository(ApplicationDbContext context, ILoggerFactory loggerFactory)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "product:")]
        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        [LoggingAspect]
        [PerformanceAspect]
        [CachingAspect(300, "all_products")]
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Where(p => !p.IsDeleted)
                .ToListAsync();
        }

        [LoggingAspect]
        [PerformanceAspect]
        public async Task<IEnumerable<Product>> SearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                throw new ArgumentException("Search term cannot be empty.", nameof(term));

            return await _context.Products
                .Where(p => !p.IsDeleted &&
                            (p.Name.Contains(term) || p.Description.Contains(term)))
                .ToListAsync();
        }

        [LoggingAspect]
        [PerformanceAspect]
        [CachingAspect(300, "products_by_category:")]
        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be empty.", nameof(category));

            return await _context.Products
                .Where(p => !p.IsDeleted && p.Category == category)
                .ToListAsync();
        }
    }
}
