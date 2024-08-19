using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTO
{
    public class UserCreateDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }
    }

    public class UserUpdateDto
    {
        public int Id { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }
    }

    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
