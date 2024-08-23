using WebAPI.DTO;

namespace WebAPI.Services.abstracts;

public interface IUserQueryService
{
    Task<UserResponseDto> GetUserByIdAsync(int id);
    Task<UserResponseDto> GetUserByEmailAsync(string email);
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<bool> ExistsByEmailAsync(string email);
}