using WebAPI.DTO;
using WebAPI.Core.Exceptions;
using WebAPI.Aspects;
using WebAPI.Data.abstracts;
using WebAPI.Services.abstracts;

namespace WebAPI.Services.Concretes
{
    public class ProductQueryService : IProductQueryService
    {
        private readonly IProductQueryRepository _productQueryRepository;

        public ProductQueryService(IProductQueryRepository productQueryRepository)
        {
            _productQueryRepository = productQueryRepository ?? throw new ArgumentNullException(nameof(productQueryRepository));
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "product:")]
        public async Task<ProductResponseDto> GetProductByIdAsync(int id)
        {
            var product = await _productQueryRepository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException($"Product with id {id} not found.");
            return product;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "products:all")]
        public async Task<IEnumerable<ProductListItemDto>> GetAllProductsAsync()
        {
            return await _productQueryRepository.GetAllAsync();
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<IEnumerable<ProductListItemDto>> SearchProductsAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                throw new BadRequestException("Search term cannot be empty.");
            return await _productQueryRepository.SearchAsync(term);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "products:category:")]
        public async Task<IEnumerable<ProductListItemDto>> GetProductsByCategoryAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new BadRequestException("Category cannot be empty.");
            return await _productQueryRepository.GetProductsByCategoryAsync(category);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<bool> IsInStockAsync(int id, int quantity)
        {
            var product = await _productQueryRepository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException($"Product with id {id} not found.");
            return product.StockQuantity >= quantity;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "products:featured")]
        public async Task<IEnumerable<ProductListItemDto>> GetFeaturedProductsAsync(int count)
        {
            // This is a placeholder implementation. In a real scenario, you might have a more complex logic to determine featured products.
            var allProducts = await _productQueryRepository.GetAllAsync();
            return allProducts.OrderByDescending(p => p.Price).Take(count);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<IEnumerable<ProductListItemDto>> GetRelatedProductsAsync(int productId, int count)
        {
            var product = await _productQueryRepository.GetByIdAsync(productId);
            if (product == null)
                throw new NotFoundException($"Product with id {productId} not found.");

            var relatedProducts = await _productQueryRepository.GetProductsByCategoryAsync(product.Category);
            return relatedProducts.Where(p => p.Id != productId).Take(count);
        }
    }
}