using System.ComponentModel.DataAnnotations;

namespace TaskTrackerAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [MaxLength(70)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        [Required]
        public string Email { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Role { get; set; } = string.Empty;

        [MaxLength(255)]
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
    }
}
