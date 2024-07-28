using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    // Request Models

    public class CreateOrderRequest
    {
        [Required]
        public string OrderId { get; set; }

        [Required]
        public string CustomerId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public List<OrderItem> Items { get; set; }

        [Required]
        public ShippingAddress ShippingAddress { get; set; }

        [Required]
        public string PaymentMethod { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TotalAmount { get; set; }
    }

    public class OrderItem
    {
        [Required]
        public string ProductId { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }

    public class ShippingAddress
    {
        [Required]
        public string Street { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string ZipCode { get; set; }

        [Required]
        public string Country { get; set; }
    }

    public class SubmitFeedbackRequest
    {
        [Required]
        public string FeedbackId { get; set; }

        [Required]
        public string CustomerId { get; set; }

        [Required]
        public string OrderReference { get; set; }

        [Required]
        public DateTime SubmissionDate { get; set; }

        [Required]
        [Range(1, 5)]
        public int OverallRating { get; set; }

        [Required]
        public FeedbackCategories Categories { get; set; }

        public string Comments { get; set; }

        [Required]
        public bool WouldRecommend { get; set; }

        public List<string> Tags { get; set; }
    }

    public class FeedbackCategories
    {
        [Range(1, 5)]
        public int ProductQuality { get; set; }

        [Range(1, 5)]
        public int Delivery { get; set; }

        [Range(1, 5)]
        public int CustomerService { get; set; }
    }

    // Response Models

    public class UserDashboardResponse
    {
        public DashboardData DashboardData { get; set; }
    }

    public class DashboardData
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public List<OrderSummary> RecentOrders { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class OrderSummary
    {
        public string OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }

    public class InventoryStatusResponse
    {
        public InventoryData InventoryData { get; set; }
    }

    public class InventoryData
    {
        public List<ProductInventory> Products { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }
    }

    public class ProductInventory
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int StockQuantity { get; set; }
        public decimal Price { get; set; }
    }

    public class CreateOrderResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string OrderId { get; set; }
    }

    public class SubmitFeedbackResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string FeedbackId { get; set; }
    }
}