using DataAccess.Models;

namespace DataAccess.Repositories.abstracts
{
    public interface IUserQueryRepository
    {
        Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task<bool> ExistsByEmailAsync(string email);
    }
}