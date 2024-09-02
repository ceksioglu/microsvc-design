using AOP.Aspects;
using DataAccess.DTO;
using DataAccess.Repositories.abstracts;
using EventHandler.Events.ProductEvents;
using EventHandler.Handlers.abstracts;
using Services.Services.abstracts;

namespace Services.Services.concretes
{
    public class ProductCommandService : IProductCommandService
    {
        private readonly IProductCommandRepository _productCommandRepository;
        private readonly IEventPublisher _eventPublisher;

        public ProductCommandService(IProductCommandRepository productCommandRepository, IEventPublisher eventPublisher)
        {
            _productCommandRepository = productCommandRepository;
            _eventPublisher = eventPublisher;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect("Admin")]
        public async Task<ProductResponseDto> CreateProductAsync(ProductCreateDto productDto)
        {
            var result = await _productCommandRepository.CreateAsync(productDto);
            await _eventPublisher.PublishAsync(new ProductCreatedEvent
            {
                ProductId = result.Id,
                Name = result.Name,
                Price = result.Price,
                StockQuantity = result.StockQuantity
            }, "product_events", "product_created");
            return result;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect("Admin")]
        public async Task<ProductResponseDto> UpdateProductAsync(int id, ProductUpdateDto productDto)
        {
            var result = await _productCommandRepository.UpdateAsync(id, productDto);
            await _eventPublisher.PublishAsync(new ProductUpdatedEvent
            {
                ProductId = result.Id,
                Name = result.Name,
                Price = result.Price,
                StockQuantity = result.StockQuantity
            }, "product_events", "product_updated");
            return result;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect("Admin")]
        public async Task<bool> DeleteProductAsync(int id)
        {
            var result = await _productCommandRepository.DeleteAsync(id);
            if (result)
            {
                await _eventPublisher.PublishAsync(new ProductDeletedEvent
                {
                    ProductId = id
                }, "product_events", "product_deleted");
            }
            return result;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect("Admin")]
        public async Task<bool> UpdateStockQuantityAsync(int id, int quantity)
        {
            var updateDto = new ProductUpdateDto { Id = id, StockQuantity = quantity };
            var result = await _productCommandRepository.UpdateAsync(id, updateDto);
            if (result != null)
            {
                await _eventPublisher.PublishAsync(new ProductStockUpdatedEvent
                {
                    ProductId = id,
                    NewStockQuantity = quantity
                }, "product_events", "stock_updated");
                return true;
            }
            return false;
        }
    }
}