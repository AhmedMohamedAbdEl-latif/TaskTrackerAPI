using System.ComponentModel.DataAnnotations;
using TaskTrackerAPI.Validation;

namespace TaskTrackerAPI.DTO
{
    public class UserDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [NotWhitespace(ErrorMessage = "Name cannot be empty or whitespace.")]
        [MaxLength(70)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
