using WebAPI.Aspects;
using WebAPI.Data.abstracts;
using WebAPI.DTO;
using WebAPI.Services.abstracts;
using WebAPI.Packages.RabbitMQ.abstracts;

namespace WebAPI.Services.concretes
{
    public class CartCommandService : ICartCommandService
    {
        private readonly ICartCommandRepository _cartCommandRepository;
        private readonly IRabbitMQService _rabbitMQService;

        public CartCommandService(ICartCommandRepository cartCommandRepository, IRabbitMQService rabbitMQService)
        {
            _cartCommandRepository = cartCommandRepository;
            _rabbitMQService = rabbitMQService;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<CartResponseDto> AddItemToCartAsync(int userId, CartItemCreateDto itemDto)
        {
            var result = await _cartCommandRepository.AddItemAsync(userId, itemDto);
            await _rabbitMQService.PublishMessage("cart_events", "item_added", new { UserId = userId, Item = itemDto });
            return result;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<CartResponseDto> UpdateCartItemAsync(int userId, int productId, CartItemUpdateDto itemDto)
        {
            var result = await _cartCommandRepository.UpdateItemAsync(userId, productId, itemDto);
            await _rabbitMQService.PublishMessage("cart_events", "item_updated", new { UserId = userId, ProductId = productId, Item = itemDto });
            return result;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<bool> RemoveItemFromCartAsync(int userId, int productId)
        {
            var result = await _cartCommandRepository.RemoveItemAsync(userId, productId);
            if (result)
            {
                await _rabbitMQService.PublishMessage("cart_events", "item_removed", new { UserId = userId, ProductId = productId });
            }
            return result;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<bool> ClearCartAsync(int userId)
        {
            var result = await _cartCommandRepository.ClearCartAsync(userId);
            if (result)
            {
                await _rabbitMQService.PublishMessage("cart_events", "cart_cleared", new { UserId = userId });
            }
            return result;
        }
    }
}