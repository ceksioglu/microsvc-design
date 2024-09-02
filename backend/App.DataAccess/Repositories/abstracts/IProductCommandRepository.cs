using DataAccess.DTO;

namespace DataAccess.Repositories.abstracts
{
    public interface IProductCommandRepository
    {
        Task<ProductResponseDto> CreateAsync(ProductCreateDto productDto);
        Task<ProductResponseDto> UpdateAsync(int id, ProductUpdateDto productDto);
        Task<bool> DeleteAsync(int id);
    }
}
