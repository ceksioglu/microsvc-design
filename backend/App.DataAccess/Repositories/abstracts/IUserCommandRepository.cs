using DataAccess.Models;

namespace DataAccess.Repositories.abstracts
{
    public interface IUserCommandRepository
    {
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(int id, User user);
        Task<bool> DeleteAsync(int id);
    }
}