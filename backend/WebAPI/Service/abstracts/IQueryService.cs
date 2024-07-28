using WebAPI.Models;

namespace WebAPI.Service.abstracts
{
    /// <summary>
    /// Defines the contract for querying operations in the e-commerce system.
    /// </summary>
    public interface IQueryService
    {
        Task<DashboardData> GetUserDashboardAsync();
        Task<List<Product>> GetInventoryStatusAsync();
    }
}