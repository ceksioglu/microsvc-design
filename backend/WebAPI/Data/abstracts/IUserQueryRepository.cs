using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.DTO;

namespace WebAPI.Data.abstracts
{
    public interface IUserQueryRepository
    {
        Task<UserResponseDto> GetByIdAsync(int id);
        Task<UserResponseDto> GetByEmailAsync(string email);
        Task<IEnumerable<UserResponseDto>> GetAllAsync();
        Task<bool> ExistsByEmailAsync(string email);
    }
}
