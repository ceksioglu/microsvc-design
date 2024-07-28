using WebAPI.Models;

namespace WebAPI.Service.abstracts
{
    /// <summary>
    /// Defines the contract for querying operations in the e-commerce system.
    /// </summary>
    public interface IQueryService
    {
        /// <summary>
        /// Retrieves the user dashboard data.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user dashboard data.</returns>
        Task<DashboardData> GetUserDashboardAsync();

        /// <summary>
        /// Retrieves the current inventory status.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the inventory status data.</returns>
        Task<InventoryData> GetInventoryStatusAsync();
    }
}