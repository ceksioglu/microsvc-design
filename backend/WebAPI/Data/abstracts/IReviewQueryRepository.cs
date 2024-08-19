using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.DTO;

namespace WebAPI.Data.abstracts
{
    public interface IReviewQueryRepository
    {
        Task<ReviewResponseDto> GetByIdAsync(int id);
        Task<IEnumerable<ReviewResponseDto>> GetByProductIdAsync(int productId);
        Task<IEnumerable<ReviewResponseDto>> GetByUserIdAsync(int userId);
        Task<double> GetAverageRatingByProductIdAsync(int productId);
    }
}
