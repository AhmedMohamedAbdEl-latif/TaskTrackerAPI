using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using TaskTrackerAPI.Repository.Auth;
using TaskTrackerAPI.UnitOfWork.Auth;

namespace TaskTrackerAPI.Services.Auth
{
    public class LoginAttemptService : ILoginAttemptService
    {
        private const int MaxFailedAttempts = 5;
        private const int MaxRetries = 3;
        private static readonly TimeSpan AttemptWindow =
            TimeSpan.FromMinutes(15);
        private static readonly TimeSpan LockoutDuration =
            TimeSpan.FromMinutes(15);
        private readonly ILoginAttemptUnitOfWorkFactory _factory;

        public LoginAttemptService(ILoginAttemptUnitOfWorkFactory factory)
        {
            _factory = factory;
        }
        public async Task<LoginAttemptResult?> CheckLockoutAsync(
            string email,
            Guid operationId,
            CancellationToken cancellationToken = default)
        {

            var emailHash = GenerateEmailHash(email);

            await using var uow = await _factory.CreateAsync(cancellationToken);

            await uow.BeginTransactionAsync(cancellationToken);

            try
            {
                await AcquireEmailLockAsync(uow, emailHash, cancellationToken);

                var existingOperation = await uow.Repository.GetOperationByIdAsync(operationId, cancellationToken);

                if (existingOperation != null)
                {
                    ValidateOperationEmail(existingOperation, emailHash);

                    return ToResult(existingOperation);
                }

                var loginAttempt = await uow.Repository.GetByEmailHashAsync(
     emailHash,
     cancellationToken);

                if (loginAttempt?.LockoutEnd is DateTime lockoutEnd)
                {
                    if (lockoutEnd > DateTime.UtcNow)
                    {
                        var result = new LoginAttemptResult
                        {
                            Outcome = LoginAttemptOutcome.LockedOut,
                            LockoutEnd = lockoutEnd
                        };

                        uow.Repository.CreateOperation(
                            CreateOperation(operationId, emailHash, result));

                        await uow.SaveChangesAsync(cancellationToken);
                        await uow.CommitTransactionAsync(cancellationToken);

                        return result;
                    }

                    // Lockout has expired. Reset the attempt state.
                    loginAttempt.FailedAttempts = 0;
                    loginAttempt.LockoutEnd = null;
                    loginAttempt.WindowStartedAt = DateTime.UtcNow;

                    await uow.SaveChangesAsync(cancellationToken);
                }

                await uow.CommitTransactionAsync(cancellationToken);

                return null;
            }
            catch
            {
                await TryRollbackAsync(uow, cancellationToken);
                throw;
            }

        }
        public async Task<LoginAttemptResult> RecordFailedAttemptAsync(
            string email,
            Guid operationId,
            CancellationToken cancellationToken = default)
        {
            var emailHash = GenerateEmailHash(email);

            return await ExecuteWithRetryAsync(
                 emailHash,
                 operationId,
                 async uow =>
                 {
                     var loginAttempt = await uow.Repository.GetByEmailHashAsync(emailHash, cancellationToken);

                     // Re-check lockout inside the same transaction and application lock.
                     // CheckLockoutAsync and password verification happen earlier, so another
                     // request may have locked the account in the meantime.
                     var now = DateTime.UtcNow;
                     if (loginAttempt?.LockoutEnd is DateTime activeLockoutEnd && activeLockoutEnd > now)
                     {
                         var lockedResult = new LoginAttemptResult
                         {
                             Outcome = LoginAttemptOutcome.LockedOut,
                             LockoutEnd = activeLockoutEnd
                         };

                         uow.Repository.CreateOperation(CreateOperation(operationId, emailHash, lockedResult));
                         return lockedResult;
                     }

                     if (loginAttempt?.LockoutEnd is DateTime expiredLockoutEnd && expiredLockoutEnd <= now)
                     {
                         loginAttempt.FailedAttempts = 0;
                         loginAttempt.LockoutEnd = null;
                         loginAttempt.WindowStartedAt = now;
                     }

                     if (loginAttempt == null)
                     {
                         loginAttempt = new LoginAttempt
                         {
                             EmailHash = emailHash,
                             FailedAttempts = 0,
                             WindowStartedAt = now,
                         };

                         uow.Repository.Create(loginAttempt);
                     }

                     // Start a new attempt window if the old one expired.
                     if (now - loginAttempt.WindowStartedAt >= AttemptWindow)
                     {
                         loginAttempt.FailedAttempts = 0;
                         loginAttempt.WindowStartedAt = now;
                         loginAttempt.LockoutEnd = null;
                     }

                     loginAttempt.FailedAttempts++;
                     loginAttempt.LastFailedAttempt = now;

                     LoginAttemptResult result;

                     if (loginAttempt.FailedAttempts >= MaxFailedAttempts)
                     {
                         loginAttempt.LockoutEnd = now.Add(LockoutDuration);

                         result = new LoginAttemptResult
                         {
                             Outcome = LoginAttemptOutcome.LockedOut,
                             LockoutEnd = loginAttempt.LockoutEnd
                         };
                     }
                     else
                     {
                         result = new LoginAttemptResult
                         {
                             Outcome = LoginAttemptOutcome.InvalidCredentials,
                             LockoutEnd = null
                         };
                     }

                     uow.Repository.CreateOperation(CreateOperation(operationId, emailHash, result));

                     return result;

                 }, cancellationToken);

        }

        public async Task<LoginAttemptResult> ResetAttemptsAsync(
            string email,
            Guid operationId,
            CancellationToken cancellationToken = default)
        {
            var emailHash = GenerateEmailHash(email);

            return await ExecuteWithRetryAsync(
                 emailHash,
                 operationId,
                 async uow =>
                 {
                     var loginAttempt = await uow.Repository.GetByEmailHashAsync(emailHash, cancellationToken);
                     var now = DateTime.UtcNow;

                     // A different request may have activated lockout after the initial
                     // CheckLockoutAsync call but before this successful-password path.
                     if (loginAttempt?.LockoutEnd is DateTime activeLockoutEnd && activeLockoutEnd > now)
                     {
                         var lockedResult = new LoginAttemptResult
                         {
                             Outcome = LoginAttemptOutcome.LockedOut,
                             LockoutEnd = activeLockoutEnd
                         };

                         uow.Repository.CreateOperation(CreateOperation(operationId, emailHash, lockedResult));
                         return lockedResult;
                     }

                     if (loginAttempt != null)
                     {
                         loginAttempt.FailedAttempts = 0;
                         loginAttempt.LockoutEnd = null;
                         loginAttempt.WindowStartedAt = now;
                     }

                     var result = new LoginAttemptResult
                     {
                         Outcome = LoginAttemptOutcome.Succeeded,
                         LockoutEnd = null
                     };

                     uow.Repository.CreateOperation(CreateOperation(operationId, emailHash, result));

                     return result;

                 },
                 cancellationToken);
        }

        private static string GenerateEmailHash(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();

            var bytes = Encoding.UTF8.GetBytes(normalizedEmail);
            var hash = SHA256.HashData(bytes);

            return Convert.ToHexString(hash);
        }

        private static async Task AcquireEmailLockAsync(
            LoginAttemptUnitOfWork uow,
            string emailHash,
            CancellationToken cancellationToken
            )
        {
            await uow.AcquireApplicationLockAsync(
                $"LoginAttempt:{emailHash}", cancellationToken
                );
        }

        private static void ValidateOperationEmail(LoginAttemptOperation operation, string emailHash)
        {
            if (!string.Equals(operation.EmailHash, emailHash, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("OperationId is already associated with another email.");
            }
        }

        private static LoginAttemptResult ToResult(LoginAttemptOperation operation)
        {
            return new LoginAttemptResult
            {
                Outcome = operation.Outcome,
                LockoutEnd = operation.LockoutEnd
            };
        }

        private static LoginAttemptOperation CreateOperation(
            Guid operationId,
            string emailHash,
            LoginAttemptResult result)
        {

            return new LoginAttemptOperation
            {
                OperationId = operationId,
                EmailHash = emailHash,
                Outcome = result.Outcome,
                LockoutEnd = result.LockoutEnd,
                CreatedAt = DateTime.UtcNow
            };

        }

        private static async Task TryRollbackAsync(LoginAttemptUnitOfWork uow, CancellationToken cancellationToken)
        {
            try
            {
                await uow.RollbackTransactionAsync(cancellationToken);
            }
            catch
            {

            }
        }

        private async Task<LoginAttemptResult> ExecuteWithRetryAsync(
            string emailHash,
            Guid operationId,
            Func<LoginAttemptUnitOfWork, Task<LoginAttemptResult>> operation,
            CancellationToken cancellationToken
            )
        {
            for (var attempt = 1; attempt <= MaxRetries; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var commitStarted = false;

                try
                {
                    await using var uow = await _factory.CreateAsync(cancellationToken);

                    await uow.BeginTransactionAsync(cancellationToken);

                    await AcquireEmailLockAsync(uow, emailHash, cancellationToken);

                    var existingOperation = await uow.Repository.GetOperationByIdAsync(operationId, cancellationToken);

                    if (existingOperation != null)
                    {
                        ValidateOperationEmail(existingOperation, emailHash);
                        return ToResult(existingOperation);
                    }

                    var result = await operation(uow);

                    await uow.SaveChangesAsync(cancellationToken);

                    commitStarted = true;

                    await uow.CommitTransactionAsync(cancellationToken);

                    return result;
                }
                catch (Exception ex) when (commitStarted && !cancellationToken.IsCancellationRequested)
                {
                    // Commit may have succeeded even if the client received an error.
                    var recoveredResult = await TryRecoverOperationAsync(emailHash, operationId, cancellationToken);

                    if (recoveredResult != null)
                        return recoveredResult;

                    if (attempt == MaxRetries || !IsDeadlock(ex))
                        throw;
                }
                catch (Exception ex) when (
                !commitStarted
                && attempt < MaxRetries
                && IsDeadlock(ex)
                && !cancellationToken.IsCancellationRequested)
                {

                }
            }

            throw new InvalidOperationException(
              "Login attempt operation could not be completed.");
        }

        private async Task<LoginAttemptResult?> TryRecoverOperationAsync(
            string emailHash,
            Guid operationId,
            CancellationToken cancellationToken)
        {
            await using var uow = await _factory.CreateAsync(cancellationToken);

            await uow.BeginTransactionAsync(cancellationToken);

            try
            {
                await AcquireEmailLockAsync(uow, emailHash, cancellationToken);

                var existingOperation = await uow.Repository.GetOperationByIdAsync(operationId, cancellationToken);

                if (existingOperation != null)
                {
                    ValidateOperationEmail(existingOperation, emailHash);
                    return ToResult(existingOperation);
                }

                // No operation was committed. The caller may retry
                // with the same OperationId.
                return null;
            }
            catch
            {
                await TryRollbackAsync(uow, cancellationToken);
                throw;
            }
        }

        private static bool IsDeadlock(Exception exception)
        {
            if (exception is ApplicationLockException appLockException)
            {
                return appLockException.ErrorType == ApplicationLockErrorType.Deadlock;
            }

            if (exception is SqlException sqlException)
            {
                return sqlException.Number == 1205;
            }

            return exception.InnerException != null &&
                IsDeadlock(exception.InnerException);
        }
    }
}
