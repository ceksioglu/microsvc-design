using DataAccess.DTO;

namespace DataAccess.Repositories.abstracts
{
    public interface IReviewCommandRepository
    {
        Task<ReviewResponseDto> CreateAsync(ReviewCreateDto reviewDto);
        Task<ReviewResponseDto> UpdateAsync(int id, ReviewUpdateDto reviewDto);
        Task<bool> DeleteAsync(int id);
    }
}
