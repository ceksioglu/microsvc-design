using WebAPI.Core.Exceptions;
using WebAPI.Aspects;
using WebAPI.Data.abstracts;
using WebAPI.DTO;
using WebAPI.Services.abstracts;
using WebAPI.Packages.RabbitMQ.abstracts;

namespace WebAPI.Services.Concretes
{
    public class ProductCommandService : IProductCommandService
    {
        private readonly IProductCommandRepository _productCommandRepository;
        private readonly IRabbitMQService _rabbitMQService;

        public ProductCommandService(IProductCommandRepository productCommandRepository, IRabbitMQService rabbitMQService)
        {
            _productCommandRepository = productCommandRepository ?? throw new ArgumentNullException(nameof(productCommandRepository));
            _rabbitMQService = rabbitMQService ?? throw new ArgumentNullException(nameof(rabbitMQService));
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect("Admin")]
        public async Task<ProductResponseDto> CreateProductAsync(ProductCreateDto productDto)
        {
            if (productDto == null)
                throw new BadRequestException("Product data is required.");

            var createdProduct = await _productCommandRepository.CreateAsync(productDto);
            await _rabbitMQService.PublishMessage("product_events", "product_created", createdProduct);
            return createdProduct;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect("Admin")]
        public async Task<ProductResponseDto> UpdateProductAsync(int id, ProductUpdateDto productDto)
        {
            if (productDto == null)
                throw new BadRequestException("Product update data is required.");

            var updatedProduct = await _productCommandRepository.UpdateAsync(id, productDto);
            if (updatedProduct == null)
                throw new NotFoundException($"Product with id {id} not found.");

            await _rabbitMQService.PublishMessage("product_events", "product_updated", updatedProduct);
            return updatedProduct;
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
                await _rabbitMQService.PublishMessage("product_events", "product_deleted", new { ProductId = id });
            }
            return result;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect("Admin")]
        public async Task<bool> UpdateStockQuantityAsync(int id, int quantity)
        {
            var updateDto = new ProductUpdateDto
            {
                Id = id,
                StockQuantity = quantity
            };
            var updatedProduct = await _productCommandRepository.UpdateAsync(id, updateDto);
            if (updatedProduct != null)
            {
                await _rabbitMQService.PublishMessage("product_events", "stock_updated", new { ProductId = id, NewQuantity = quantity });
                return true;
            }
            return false;
        }
    }
}