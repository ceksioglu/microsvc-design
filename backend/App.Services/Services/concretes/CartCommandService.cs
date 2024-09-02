using AOP.Aspects;
using DataAccess.DTO;
using DataAccess.Repositories.abstracts;
using Services.Services.abstracts;
using EventHandler.Events.CartEvents;
using EventHandler.Handlers.abstracts;

namespace Services.Services.concretes
{
    public class CartCommandService : ICartCommandService
    {
        private readonly ICartCommandRepository _cartCommandRepository;
        private readonly IEventPublisher _eventPublisher;

        public CartCommandService(ICartCommandRepository cartCommandRepository, IEventPublisher eventPublisher)
        {
            _cartCommandRepository = cartCommandRepository;
            _eventPublisher = eventPublisher;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<CartResponseDto> AddItemToCartAsync(int userId, CartItemCreateDto itemDto)
        {
            var result = await _cartCommandRepository.AddItemAsync(userId, itemDto);
            await _eventPublisher.PublishAsync(new CartItemAddedEvent
            {
                UserId = userId,
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity
            }, "cart_events", "item_added");
            return result;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<CartResponseDto> UpdateCartItemAsync(int userId, int productId, CartItemUpdateDto itemDto)
        {
            var result = await _cartCommandRepository.UpdateItemAsync(userId, productId, itemDto);
            await _eventPublisher.PublishAsync(new CartItemAddedEvent
            {
                UserId = userId,
                ProductId = productId,
                Quantity = itemDto.Quantity
            }, "cart_events", "item_updated");
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
                await _eventPublisher.PublishAsync(new CartItemRemovedEvent
                {
                    UserId = userId,
                    ProductId = productId
                }, "cart_events", "item_removed");
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
                await _eventPublisher.PublishAsync(new CartClearedEvent
                {
                    UserId = userId
                }, "cart_events", "cart_cleared");
            }
            return result;
        }
    }
}