using AOP.Aspects;
using AutoMapper;
using DataAccess.DTO;
using DataAccess.Models;
using DataAccess.Repositories.abstracts;

namespace DataAccess.Repositories.concretes
{
    public class OrderCommandRepository : IOrderCommandRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public OrderCommandRepository(
            ApplicationDbContext context,
            IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<OrderResponseDto> CreateAsync(OrderCreateDto orderDto)
        {
            if (orderDto == null)
                throw new ArgumentNullException(nameof(orderDto));

            var order = _mapper.Map<Order>(orderDto);
            order.OrderDate = DateTime.UtcNow;
            order.Status = "Pending"; // Initial status

            // Calculate total amount
            order.TotalAmount = 0;
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                    throw new KeyNotFoundException($"Product with id {item.ProductId} not found.");
                
                item.Price = product.Price;
                order.TotalAmount += item.Price * item.Quantity;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return _mapper.Map<OrderResponseDto>(order);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<OrderResponseDto> UpdateAsync(int id, OrderUpdateDto orderDto)
        {
            if (orderDto == null)
                throw new ArgumentNullException(nameof(orderDto));

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                throw new KeyNotFoundException($"Order with id {id} not found.");

            order.Status = orderDto.Status;

            await _context.SaveChangesAsync();

            return _mapper.Map<OrderResponseDto>(order);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<bool> DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return false;

            order.IsDeleted = true;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}