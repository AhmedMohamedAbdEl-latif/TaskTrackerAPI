using Microsoft.EntityFrameworkCore;

namespace TaskTrackerAPI.Services.Auth
{
    public sealed class LoginAttemptRetentionService(
        IDbContextFactory<TaskTrackerContext> contextFactory,
        ILogger<LoginAttemptRetentionService> logger)
        : BackgroundService
    {
        private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(24);
        private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(90);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(InitialDelay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Login attempt retention cleanup failed.");
                }

                try
                {
                    await Task.Delay(CleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }

        private async Task CleanupAsync(CancellationToken cancellationToken)
        {
            var cutoff = DateTime.UtcNow.Subtract(RetentionPeriod);
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

            var removedOperations = await db.LoginAttemptOperations
                .Where(operation => operation.CreatedAt < cutoff)
                .ExecuteDeleteAsync(cancellationToken);

            var removedAttempts = await db.LoginAttempts
                .Where(attempt => attempt.LastFailedAttempt < cutoff &&
                    (!attempt.LockoutEnd.HasValue || attempt.LockoutEnd.Value < DateTime.UtcNow))
                .ExecuteDeleteAsync(cancellationToken);

            if (removedOperations > 0 || removedAttempts > 0)
            {
                logger.LogInformation(
                    "Removed expired authentication records. Operations: {Operations}; Attempts: {Attempts}.",
                    removedOperations,
                    removedAttempts);
            }
        }
    }
}
