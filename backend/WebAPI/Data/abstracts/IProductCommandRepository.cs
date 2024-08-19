using System.Threading.Tasks;
using WebAPI.DTO;

namespace WebAPI.Data.abstracts
{
    public interface IProductCommandRepository
    {
        Task<ProductResponseDto> CreateAsync(ProductCreateDto productDto);
        Task<ProductResponseDto> UpdateAsync(int id, ProductUpdateDto productDto);
        Task<bool> DeleteAsync(int id);
    }
}
