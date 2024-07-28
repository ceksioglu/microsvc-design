using Newtonsoft.Json;
using StackExchange.Redis;
using WebAPI.Models;
using WebAPI.Service.abstracts;
using WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Service.concretes
{
    /// <summary>
    /// Implements the IQueryService interface for handling read operations.
    /// This service utilizes Redis for caching and falls back to the database when necessary.
    /// </summary>
    public class QueryService : IQueryService
    {
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the QueryService class.
        /// </summary>
        /// <param name="redisConnection">The Redis connection multiplexer for caching operations.</param>
        /// <param name="dbContext">The database context for Entity Framework operations.</param>
        public QueryService(IConnectionMultiplexer redisConnection, ApplicationDbContext dbContext)
        {
            _redisConnection = redisConnection;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves the user dashboard data asynchronously.
        /// </summary>
        /// <returns>The dashboard data for the user.</returns>
        public async Task<DashboardData> GetUserDashboardAsync()
        {
            var db = _redisConnection.GetDatabase();
            var dashboardData = await db.StringGetAsync("user_dashboard");
            
            if (!dashboardData.IsNullOrEmpty)
            {
                return JsonConvert.DeserializeObject<DashboardData>(dashboardData);
            }

            // If not in cache, fetch from database
            var recentOrders = await _dbContext.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new OrderSummary
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                })
                .ToListAsync();

            var totalOrders = await _dbContext.Orders.CountAsync();
            var totalSpent = await _dbContext.Orders.SumAsync(o => o.TotalAmount);

            var dashboard = new DashboardData
            {
                RecentOrders = recentOrders,
                TotalOrders = totalOrders,
                TotalSpent = totalSpent
            };

            // Cache the result
            await db.StringSetAsync("user_dashboard", JsonConvert.SerializeObject(dashboard), TimeSpan.FromMinutes(5));

            return dashboard;
        }

        /// <summary>
        /// Retrieves the current inventory status asynchronously.
        /// </summary>
        /// <returns>A list of products representing the current inventory.</returns>
        public async Task<List<Product>> GetInventoryStatusAsync()
        {
            var db = _redisConnection.GetDatabase();
            var inventoryData = await db.StringGetAsync("inventory_status");
            
            if (!inventoryData.IsNullOrEmpty)
            {
                return JsonConvert.DeserializeObject<List<Product>>(inventoryData);
            }

            // If not in cache, fetch from database
            var products = await _dbContext.Products.ToListAsync();

            // Cache the result
            await db.StringSetAsync("inventory_status", JsonConvert.SerializeObject(products), TimeSpan.FromMinutes(5));

            return products;
        }
    }
}