using System.ComponentModel.DataAnnotations;

namespace TaskTrackerAPI.DTO.Auth
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(128)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(32)]
        [System.ComponentModel.DataAnnotations.RegularExpression("^([0-9]{6}|[A-Fa-f0-9]{32})$")]
        public string? TwoFactorCode { get; set; }
    }
}
