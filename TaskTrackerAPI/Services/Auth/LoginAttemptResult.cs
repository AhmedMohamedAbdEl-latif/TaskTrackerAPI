namespace TaskTrackerAPI.Services.Auth
{
    public class LoginAttemptResult
    {
        public LoginAttemptOutcome Outcome { get; set; }
        public DateTime? LockoutEnd { get; set; }
    }
}
