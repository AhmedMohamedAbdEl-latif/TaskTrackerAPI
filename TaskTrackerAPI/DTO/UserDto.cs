using System.ComponentModel.DataAnnotations;

namespace TaskTrackerAPI.DTO
{
    public class UserDto
    {
        [Required]
        [MaxLength(70)]
        public string Name { get; set; }
        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }
        [MaxLength(50)]
        public string Role { get; set; }
    }
}
