using System.ComponentModel.DataAnnotations;

namespace DataAccess.DTO
{
    public class OrderCreateDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public List<OrderItemDto> Items { get; set; }

        [Required]
        public ShippingAddressDto ShippingAddress { get; set; }
    }

    public class OrderUpdateDto
    {
        public int Id { get; set; }
        public string Status { get; set; }
    }

    public class OrderResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public ShippingAddressDto ShippingAddress { get; set; }
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class ShippingAddressDto
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(200)]
        public string AddressLine1 { get; set; }

        [StringLength(200)]
        public string AddressLine2 { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [StringLength(100)]
        public string State { get; set; }

        [Required]
        [StringLength(20)]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; }
    }
}
