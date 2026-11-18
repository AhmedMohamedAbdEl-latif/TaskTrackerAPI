using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
namespace TaskTrackerAPI.Common
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IProblemDetailsService problemDetailsService)
        {
            _logger = logger;
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is DbUpdateException dpUpdateException
                && IsDuplicateEmailException(dpUpdateException))
            {
                _logger.LogWarning(dpUpdateException, "Duplicate email detected while saving user.");

                httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

                var conflictProblem = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflict",
                    Detail = "This email address already exists."
                };

                conflictProblem.Extensions["traceId"] = httpContext.TraceIdentifier;

                await _problemDetailsService.WriteAsync(
                    new ProblemDetailsContext
                    {
                        HttpContext = httpContext,
                        ProblemDetails = conflictProblem
                    });

                return true;
            }

            _logger.LogError(
                exception,
                "An unexpected error occurred. TraceId: {TraceId}",
                httpContext.TraceIdentifier
                );

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal server error",
                Detail = "An unexpected error occurred. Please try again later."
            };

            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            await _problemDetailsService.WriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    ProblemDetails = problemDetails
                });

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
    }
}
