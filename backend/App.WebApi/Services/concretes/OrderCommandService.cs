
using AOP.Aspects;
using Core.RabbitMQ.abstracts;
using WebAPI.Data.abstracts;
using WebAPI.DTO;
using WebAPI.Services.abstracts;

namespace WebAPI.Services.concretes
{
    public class OrderCommandService : IOrderCommandService
    {
        private readonly IOrderCommandRepository _orderCommandRepository;
        private readonly IRabbitMQService _rabbitMQService;

        public OrderCommandService(IOrderCommandRepository orderCommandRepository, IRabbitMQService rabbitMQService)
        {
            _orderCommandRepository = orderCommandRepository;
            _rabbitMQService = rabbitMQService;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<OrderResponseDto> CreateOrderAsync(OrderCreateDto orderDto)
        {
            var result = await _orderCommandRepository.CreateAsync(orderDto);
            await _rabbitMQService.PublishMessage("order_events", "order_created", result);
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
            await _rabbitMQService.PublishMessage("order_events", "order_status_updated", new { OrderId = orderId, NewStatus = newStatus });
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
                await _rabbitMQService.PublishMessage("order_events", "order_cancelled", new { OrderId = orderId });
            }
            return result;
        }
    }
}