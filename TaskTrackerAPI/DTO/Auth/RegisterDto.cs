using System.ComponentModel.DataAnnotations;
using TaskTrackerAPI.Validation;

namespace TaskTrackerAPI.DTO.Auth
{
    public class RegisterDto
    {
        [Required]
        [NotWhitespace(ErrorMessage = "Name cannot be empty or whitespace.")]
        [MaxLength(70)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(128)]
        public string Password { get; set; } = string.Empty;
    }
}
