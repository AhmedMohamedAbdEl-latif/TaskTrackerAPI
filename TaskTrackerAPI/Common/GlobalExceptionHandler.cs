using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace TaskTrackerAPI.Common
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // A browser or proxy can cancel a request when its user navigates
            // away. That is normal request flow, not a server failure.
            if (exception is OperationCanceledException &&
                httpContext.RequestAborted.IsCancellationRequested)
            {
                _logger.LogDebug(
                    "Request was cancelled by the client. TraceId: {TraceId}",
                    httpContext.TraceIdentifier);
                return true;
            }

            if (httpContext.Response.HasStarted)
            {
                _logger.LogError(
                    exception,
                    "An exception occurred after the response started. TraceId: {TraceId}",
                    httpContext.TraceIdentifier);
                return false;
            }

            var statusCode = StatusCodes.Status500InternalServerError;
            var errorType = ServiceErrorType.InternalServerError;
            var message = "An unexpected error occurred. Please try again later.";

            if (exception is DbUpdateException dbUpdateException &&
                IsDuplicateEmailException(dbUpdateException))
            {
                _logger.LogWarning(
                    dbUpdateException,
                    "Duplicate email detected while saving user.");
                statusCode = StatusCodes.Status409Conflict;
                errorType = ServiceErrorType.Conflict;
                message = "This email address already exists.";
            }
            else if (exception is DbUpdateException foreignKeyException &&
                     IsForeignKeyConstraintException(foreignKeyException))
            {
                _logger.LogWarning(
                    foreignKeyException,
                    "A database relationship prevented the requested change.");
                statusCode = StatusCodes.Status409Conflict;
                errorType = ServiceErrorType.Conflict;
                message = "The operation conflicts with related records. Refresh the data and try again.";
            }
            else
            {
                _logger.LogError(
                    exception,
                    "An unexpected error occurred. TraceId: {TraceId}",
                    httpContext.TraceIdentifier);
            }

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(
                new ServiceResult
                {
                    Success = false,
                    ErrorType = errorType,
                    Errors = new List<string> { message },
                    TraceId = httpContext.TraceIdentifier
                },
                cancellationToken);

            return true;
        }

        public static bool IsDuplicateEmailException(DbUpdateException exception)
        {
            var sqlException = exception.GetBaseException() as SqlException;

            if (sqlException == null)
                return false;

            var isUniqueViolation = sqlException.Number is 2601 or 2627;
            var isEmailIndex = sqlException.Message.Contains(
                "IX_Users_Email",
                StringComparison.OrdinalIgnoreCase);

            return isUniqueViolation && isEmailIndex;
        }

        public static bool IsForeignKeyConstraintException(DbUpdateException exception)
        {
            return exception.GetBaseException() is SqlException { Number: 547 };
        }
    }
}
