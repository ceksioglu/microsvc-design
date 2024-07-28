using Newtonsoft.Json;
using StackExchange.Redis;
using WebAPI.Models;
using WebAPI.Service.abstracts;

namespace WebAPI.Service.concretes
{
    public class QueryService : IQueryService
    {
        private readonly IConnectionMultiplexer _redisConnection;

        public QueryService(IConnectionMultiplexer redisConnection)
        {
            _redisConnection = redisConnection;
        }

        public async Task<DashboardData> GetUserDashboardAsync()
        {
            var db = _redisConnection.GetDatabase();
            var dashboardData = await db.StringGetAsync("user_dashboard");
            return dashboardData.IsNullOrEmpty ? null : JsonConvert.DeserializeObject<DashboardData>(dashboardData);
        }

        public async Task<InventoryData> GetInventoryStatusAsync()
        {
            var db = _redisConnection.GetDatabase();
            var inventoryData = await db.StringGetAsync("inventory_status");
            return inventoryData.IsNullOrEmpty ? null : JsonConvert.DeserializeObject<InventoryData>(inventoryData);
        }
    }
}