namespace WebAPI.Models
{
    public class DashboardData
    {
        public List<OrderSummary> RecentOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalOrders { get; set; }
    }

    public class OrderSummary
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }
}