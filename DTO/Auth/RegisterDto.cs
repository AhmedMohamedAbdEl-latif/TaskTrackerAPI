using System.ComponentModel.DataAnnotations;

namespace TaskTrackerAPI.DTO.Auth
{
    public class RegisterDto
    {
        [Required]
        [MaxLength(70)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
