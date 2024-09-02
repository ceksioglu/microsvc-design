using DataAccess.DTO;

namespace Services.Services.abstracts;

public interface IUserQueryService
{
    Task<UserResponseDto> GetUserByIdAsync(int id);
    Task<UserResponseDto> GetUserByEmailAsync(string email);
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<bool> ExistsByEmailAsync(string email);
}