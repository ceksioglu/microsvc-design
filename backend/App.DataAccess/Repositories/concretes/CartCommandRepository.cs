using AOP.Aspects;
using AutoMapper;
using DataAccess.DTO;
using DataAccess.Models;
using DataAccess.Repositories.abstracts;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.concretes
{
    public class CartCommandRepository : ICartCommandRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CartCommandRepository(
            ApplicationDbContext context,
            IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<CartResponseDto> AddItemAsync(int userId, CartItemCreateDto itemDto)
        {
            var cart = await GetOrCreateCartAsync(userId);

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == itemDto.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += itemDto.Quantity;
            }
            else
            {
                var newItem = _mapper.Map<CartItem>(itemDto);
                newItem.CartId = cart.Id;
                cart.Items.Add(newItem);
            }

            await _context.SaveChangesAsync();

            return await CreateCartResponseDtoAsync(cart);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<CartResponseDto> UpdateItemAsync(int userId, int productId, CartItemUpdateDto itemDto)
        {
            var cart = await GetCartAsync(userId);
            if (cart == null)
                throw new KeyNotFoundException($"Cart not found for user {userId}");

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
                throw new KeyNotFoundException($"Item with productId {productId} not found in the cart");

            if (itemDto.Quantity == 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                item.Quantity = itemDto.Quantity;
            }

            await _context.SaveChangesAsync();

            return await CreateCartResponseDtoAsync(cart);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<bool> RemoveItemAsync(int userId, int productId)
        {
            var cart = await GetCartAsync(userId);
            if (cart == null)
                return false;

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
                return false;

            cart.Items.Remove(item);
            await _context.SaveChangesAsync();

            return true;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<bool> ClearCartAsync(int userId)
        {
            var cart = await GetCartAsync(userId);
            if (cart == null)
                return false;

            cart.Items.Clear();
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await GetCartAsync(userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            return cart;
        }

        private async Task<Cart> GetCartAsync(int userId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        private async Task<CartResponseDto> CreateCartResponseDtoAsync(Cart cart)
        {
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