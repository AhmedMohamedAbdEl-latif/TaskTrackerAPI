namespace TaskTrackerAPI.Services.Auth
{
    public interface ILoginAttemptService
    {
        Task<LoginAttemptResult?> CheckLockoutAsync(
            string email,
            Guid operationId,
            CancellationToken cancellationToken = default);

        Task<LoginAttemptResult> RecordFailedAttemptAsync(
            string email,
            Guid operationId,
            CancellationToken cancellationToken = default);
        Task<LoginAttemptResult> ResetAttemptsAsync(
            string email,
            Guid operationId,
            CancellationToken cancellationToken = default);
    }
}
