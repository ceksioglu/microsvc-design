using Microsoft.EntityFrameworkCore;
using WebAPI.Data.abstracts;
using WebAPI.Models;
using WebAPI.Packages.Redis.abstracts;

namespace WebAPI.Data.concretes
{
    public class ProductCommandRepository : IProductCommandRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IRedisService _redisService;

        public ProductCommandRepository(ApplicationDbContext context, IRedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        public async Task<Product> AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            await _redisService.DeleteAsync($"product:{product.Id}");
            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await _redisService.DeleteAsync($"product:{product.Id}");
            return product;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            product.IsDeleted = true;
            await _context.SaveChangesAsync();
            await _redisService.DeleteAsync($"product:{id}");
            return true;
        }

        public async Task<bool> UpdateStockQuantityAsync(int id, int quantity)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            product.StockQuantity = quantity;
            await _context.SaveChangesAsync();
            await _redisService.DeleteAsync($"product:{id}");
            return true;
        }
    }
}