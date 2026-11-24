using System.ComponentModel.DataAnnotations;

namespace TaskTrackerAPI.DTO.Auth
{
    public sealed class TwoFactorSetupRequest
    {
        [Required]
        [MaxLength(128)]
        public string Password { get; set; } = string.Empty;
    }
}
