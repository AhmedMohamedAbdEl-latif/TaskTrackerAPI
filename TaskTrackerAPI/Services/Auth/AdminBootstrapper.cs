using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace TaskTrackerAPI.Services.Auth
{
    public sealed class AdminBootstrapper(
        IDbContextFactory<TaskTrackerContext> contextFactory,
        PasswordHasher<User> passwordHasher,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<AdminBootstrapper> logger)
    {
        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var email = configuration["AdminBootstrap:Email"]?.Trim().ToLowerInvariant();
            var password = configuration["AdminBootstrap:Password"];
            var name = configuration["AdminBootstrap:Name"];
            var bootstrapIsUnconfigured = string.IsNullOrWhiteSpace(email) &&
                string.IsNullOrWhiteSpace(password) &&
                string.IsNullOrWhiteSpace(name);

            if (bootstrapIsUnconfigured)
            {
                if (environment.IsProduction())
                {
                    await using var productionDb = await contextFactory
                        .CreateDbContextAsync(cancellationToken);
                    var administratorExists = await productionDb.Users
                        .AnyAsync(user => user.Role == UserRole.Admin, cancellationToken);

                    if (!administratorExists)
                    {
                        throw new InvalidOperationException(
                            "No administrator exists. Configure AdminBootstrap:Name, Email, and a Password of at least 12 characters for the first production startup.");
                    }
                }

                return;
            }

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(name) || password.Length < 12)
                throw new InvalidOperationException(
                    "Configure AdminBootstrap:Name, Email, and a Password of at least 12 characters, or omit all three.");

            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            await db.Database.ExecuteSqlInterpolatedAsync($@"
                DECLARE @lockResult int;
                EXEC @lockResult = sp_getapplock
                    @Resource = N'AdminMembership',
                    @LockMode = N'Exclusive',
                    @LockOwner = N'Transaction',
                    @LockTimeout = 5000;
                IF @lockResult < 0
                    THROW 51000, 'Could not acquire the admin membership lock.', 1;",
                cancellationToken);

            if (await db.Users.AnyAsync(u => u.Role == UserRole.Admin, cancellationToken))
            {
                logger.LogInformation("Admin bootstrap skipped because an administrator already exists.");
                await transaction.CommitAsync(cancellationToken);
                return;
            }
            if (await db.Users.AnyAsync(u => u.Email == email, cancellationToken))
                throw new InvalidOperationException("The configured bootstrap email is already used by a non-admin account.");

            var admin = new User { Name = name.Trim(), Email = email, Role = UserRole.Admin };
            admin.PasswordHash = passwordHasher.HashPassword(admin, password);
            db.Users.Add(admin);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            logger.LogWarning("Created the initial administrator account {Email}. Remove AdminBootstrap credentials from configuration.", email);
        }
    }
}
