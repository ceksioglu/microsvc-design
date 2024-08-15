using WebAPI.Models;

namespace WebAPI.Data.abstracts;

public interface IProductQueryRepository
{
    Task<Product> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IEnumerable<Product>> SearchAsync(string term);
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);

}