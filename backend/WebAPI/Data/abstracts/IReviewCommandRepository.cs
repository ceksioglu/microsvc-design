using System.Threading.Tasks;
using WebAPI.DTO;

namespace WebAPI.Data.abstracts
{
    public interface IReviewCommandRepository
    {
        Task<ReviewResponseDto> CreateAsync(ReviewCreateDto reviewDto);
        Task<ReviewResponseDto> UpdateAsync(int id, ReviewUpdateDto reviewDto);
        Task<bool> DeleteAsync(int id);
    }
}
