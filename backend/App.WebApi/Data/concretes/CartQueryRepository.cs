using AOP.Aspects;
using AutoMapper;
using Core.Redis.abstracts;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data.abstracts;
using WebAPI.DTO;
using WebAPI.Models;

namespace WebAPI.Data.concretes
{
    public class CartQueryRepository : ICartQueryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IRedisService _redisService;

        public CartQueryRepository(
            ApplicationDbContext context,
            IMapper mapper,
            IRedisService redisService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _redisService = redisService ?? throw new ArgumentNullException(nameof(redisService));
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "cart:user:")]
        public async Task<CartResponseDto> GetCartByUserIdAsync(int userId)
        {
            var cart = await _context.Carts
                .AsNoTracking()
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return null;

            var cartDto = _mapper.Map<CartResponseDto>(cart);
            cartDto.TotalAmount = await CalculateTotalAmountAsync(cart);

            return cartDto;
        }

        private async Task<decimal> CalculateTotalAmountAsync(Cart cart)
        {
            decimal total = 0;
            foreach (var item in cart.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    total += product.Price * item.Quantity;
                }
            }
            return total;
        }
    }
}
