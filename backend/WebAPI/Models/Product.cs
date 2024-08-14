using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; }

        public List<string> Images { get; set; } = new List<string>();

        public List<Review> Reviews { get; set; } = new List<Review>();

        public bool IsDeleted { get; set; } = false;
    }
}