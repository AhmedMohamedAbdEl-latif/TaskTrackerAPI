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

        public UserRole Role { get; set; } = UserRole.User;

        public bool TwoFactorEnabled { get; set; }

        [MaxLength(2000)]
        public string? TwoFactorSecretProtected { get; set; }

        [MaxLength(2000)]
        public string? TwoFactorRecoveryCodeHashesJson { get; set; }

        public long? TwoFactorLastUsedTimeStep { get; set; }

        [MaxLength(255)]
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
    }

    public enum UserRole
    {
        User = 1,
        Admin = 2
    }
}
