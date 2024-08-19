using System.Threading.Tasks;
using WebAPI.DTO;

namespace WebAPI.Data.abstracts
{
    public interface IUserCommandRepository
    {
        Task<UserResponseDto> CreateAsync(UserCreateDto userDto);
        Task<UserResponseDto> UpdateAsync(int id, UserUpdateDto userDto);
        Task<bool> DeleteAsync(int id);
    }
}
