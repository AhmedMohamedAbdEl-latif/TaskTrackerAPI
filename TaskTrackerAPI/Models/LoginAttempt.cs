namespace TaskTrackerAPI.Models
{
    public class LoginAttempt
    {
        public int Id { get; set; }
        public string EmailHash { get; set; } = string.Empty;
        public int FailedAttempts { get; set; }
        public DateTime LastFailedAttempt { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public DateTime WindowStartedAt { get; set; }
    }
}
