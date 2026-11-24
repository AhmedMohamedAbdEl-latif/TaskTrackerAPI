using System.ComponentModel.DataAnnotations;

namespace TaskTrackerAPI.DTO.Auth
{
    public sealed class TwoFactorCodeDto
    {
        [Required]
        [MaxLength(32)]
        [RegularExpression("^([0-9]{6}|[A-Fa-f0-9]{32})$")]
        public string Code { get; set; } = string.Empty;
    }
}
