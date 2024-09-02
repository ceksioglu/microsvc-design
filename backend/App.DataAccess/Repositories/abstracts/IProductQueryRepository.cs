using DataAccess.DTO;

namespace DataAccess.Repositories.abstracts
{
    public interface IProductQueryRepository
    {
        Task<ProductResponseDto> GetByIdAsync(int id);
        Task<IEnumerable<ProductListItemDto>> GetAllAsync();
        Task<IEnumerable<ProductListItemDto>> SearchAsync(string term);
        Task<IEnumerable<ProductListItemDto>> GetProductsByCategoryAsync(string category);
    }
}
