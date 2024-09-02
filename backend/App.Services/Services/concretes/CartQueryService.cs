using AOP.Aspects;
using DataAccess.DTO;
using DataAccess.Repositories.abstracts;
using Services.Services.abstracts;

namespace Services.Services.concretes
{
    public class CartQueryService : ICartQueryService
    {
        private readonly ICartQueryRepository _cartQueryRepository;

        public CartQueryService(ICartQueryRepository cartQueryRepository)
        {
            _cartQueryRepository = cartQueryRepository;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        [CachingAspect(300, "cart:user:")]
        public async Task<CartResponseDto> GetCartByUserIdAsync(int userId)
        {
            return await _cartQueryRepository.GetCartByUserIdAsync(userId);
        }
    }
}
