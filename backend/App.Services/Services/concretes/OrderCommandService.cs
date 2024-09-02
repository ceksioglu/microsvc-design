using AOP.Aspects;
using DataAccess.DTO;
using DataAccess.Repositories.abstracts;
using Services.Services.abstracts;
using EventHandler.Events.OrderEvents;
using EventHandler.Handlers.abstracts;

namespace Services.Services.concretes
{
    public class OrderCommandService : IOrderCommandService
    {
        private readonly IOrderCommandRepository _orderCommandRepository;
        private readonly IEventPublisher _eventPublisher;

        public OrderCommandService(IOrderCommandRepository orderCommandRepository, IEventPublisher eventPublisher)
        {
            _orderCommandRepository = orderCommandRepository;
            _eventPublisher = eventPublisher;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<OrderResponseDto> CreateOrderAsync(OrderCreateDto orderDto)
        {
            var result = await _orderCommandRepository.CreateAsync(orderDto);
            await _eventPublisher.PublishAsync(new OrderCreatedEvent
            {
                OrderId = result.Id,
                UserId = result.UserId,
                TotalAmount = result.TotalAmount
            }, "order_events", "order_created");
            return result;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<OrderResponseDto> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            var updateDto = new OrderUpdateDto { Id = orderId, Status = newStatus };
            var result = await _orderCommandRepository.UpdateAsync(orderId, updateDto);
            await _eventPublisher.PublishAsync(new OrderStatusUpdatedEvent
            {
                OrderId = orderId,
                NewStatus = newStatus
            }, "order_events", "order_status_updated");
            return result;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var result = await _orderCommandRepository.DeleteAsync(orderId);
            if (result)
            {
                await _eventPublisher.PublishAsync(new OrderCancelledEvent
                {
                    OrderId = orderId
                }, "order_events", "order_cancelled");
            }
            return result;
        }
    }
}