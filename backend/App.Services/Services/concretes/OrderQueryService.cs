using AOP.Aspects;
using DataAccess.DTO;
using DataAccess.Repositories.abstracts;
using Services.Services.abstracts;

namespace Services.Services.concretes
{
    public class OrderQueryService : IOrderQueryService
    {
        private readonly IOrderQueryRepository _orderQueryRepository;

        public OrderQueryService(IOrderQueryRepository orderQueryRepository)
        {
            _orderQueryRepository = orderQueryRepository;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        [CachingAspect(300, "order:")]
        public async Task<OrderResponseDto> GetOrderByIdAsync(int orderId)
        {
            return await _orderQueryRepository.GetByIdAsync(orderId);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        [CachingAspect(300, "orders:user:")]
        public async Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(int userId)
        {
            return await _orderQueryRepository.GetOrdersByUserIdAsync(userId);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect("Admin")]
        [CachingAspect(300, "orders:all")]
        public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync()
        {
            return await _orderQueryRepository.GetAllAsync();
        }
    }
}
