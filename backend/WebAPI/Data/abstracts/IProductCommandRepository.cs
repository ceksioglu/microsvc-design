using WebAPI.Models;

namespace WebAPI.Data.abstracts
{
    public interface IProductCommandRepository
    {
        Task<Product> AddAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task<bool> DeleteAsync(int id);
    }
}