using System.ComponentModel.DataAnnotations;

namespace DataAccess.DTO
{
    public class CustomerSupportTicketCreateDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(500)]
        public string Issue { get; set; }
    }

    public class CustomerSupportTicketUpdateDto
    {
        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(500)]
        public string Resolution { get; set; }
    }

    public class CustomerSupportTicketResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Issue { get; set; }
        public string Status { get; set; }
        public string Resolution { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}