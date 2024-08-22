using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Data.Abstracts
{
    public interface IUserQueryRepository
    {
        Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task<bool> ExistsByEmailAsync(string email);
    }
}