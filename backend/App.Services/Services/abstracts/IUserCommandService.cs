using DataAccess.DTO;

namespace Services.Services.abstracts;

public interface IUserCommandService
{
    Task<UserResponseDto> RegisterUserAsync(UserCreateDto userDto);
    Task<UserResponseDto> UpdateUserAsync(int id, UserUpdateDto userDto);
    Task<bool> DeleteUserAsync(int id);
    Task<string> AuthenticateUserAsync(string email, string password);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}