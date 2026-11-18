using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskTrackerAPI.Models
{
    public enum LoginAttemptOutcome
    {
        InvalidCredentials = 1,
        LockedOut = 2,
        Succeeded = 3
    }

    public class LoginAttemptOperation
    {
        [Key]
        public Guid OperationId { get; set; }

        [Required]
        [MaxLength(64)]
        public string EmailHash { get; set; } = string.Empty;

        [Required]
        public LoginAttemptOutcome Outcome { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? LockoutEnd { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; }
    }
}
