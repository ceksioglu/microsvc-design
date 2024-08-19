using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data.abstracts;
using WebAPI.DTO;
using WebAPI.Models;
using WebAPI.Aspects;
using WebAPI.Packages.RabbitMQ.abstracts;

namespace WebAPI.Data.concretes
{
    public class ProductCommandRepository : IProductCommandRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IRabbitMQService _rabbitMQService;

        public ProductCommandRepository(
            ApplicationDbContext context,
            IMapper mapper,
            IRabbitMQService rabbitMQService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _rabbitMQService = rabbitMQService ?? throw new ArgumentNullException(nameof(rabbitMQService));
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<ProductResponseDto> CreateAsync(ProductCreateDto productDto)
        {
            if (productDto == null)
                throw new ArgumentNullException(nameof(productDto));

            var product = _mapper.Map<Product>(productDto);
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await PublishProductEvent("product_created", product);

            return _mapper.Map<ProductResponseDto>(product);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<ProductResponseDto> UpdateAsync(int id, ProductUpdateDto productDto)
        {
            if (productDto == null)
                throw new ArgumentNullException(nameof(productDto));

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                throw new KeyNotFoundException($"Product with id {id} not found.");

            _mapper.Map(productDto, product);
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await PublishProductEvent("product_updated", product);

            return _mapper.Map<ProductResponseDto>(product);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await PublishProductEvent("product_deleted", id);

            return true;
        }

        [LoggingAspect]
        [ExceptionAspect]
        private async Task PublishProductEvent(string eventType, object payload)
        {
            await _rabbitMQService.PublishMessage("product_events", eventType, payload);
        }
    }
}
