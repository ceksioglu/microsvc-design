using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTO
{
    public class CartItemCreateDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class CartItemUpdateDto
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class CartItemResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CartResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<CartItemResponseDto> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}