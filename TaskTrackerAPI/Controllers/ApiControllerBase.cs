using Microsoft.AspNetCore.Mvc;

namespace TaskTrackerAPI.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        protected ServiceResult<T> SuccessResult<T>(T data)
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data
            };
        }

        protected ServiceResult SuccessResult()
        {
            return new ServiceResult { Success = true };
        }

        protected IActionResult HandleError(ServiceResult result)
        {
            if (!result.ErrorType.HasValue)
            {
                result.ErrorType = ServiceErrorType.InternalServerError;
                result.Errors = new List<string> { "An unexpected error occurred. Please try again later." };
            }

            result.TraceId ??= HttpContext.TraceIdentifier;

            return result.ErrorType switch
            {
                ServiceErrorType.NotFound => NotFound(result),
                ServiceErrorType.Validation => BadRequest(result),
                ServiceErrorType.Unauthorized => Unauthorized(result),
                ServiceErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result),
                ServiceErrorType.Conflict => Conflict(result),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result),
            };
        }

        protected IActionResult ForbiddenError()
        {
            var validationResult = new ServiceResult
            {
                Success = false,
                ErrorType = ServiceErrorType.Forbidden,
                Errors = new List<string> { "You are not allowed to perform this action." }
            };
            validationResult.TraceId = HttpContext.TraceIdentifier;
            return HandleError(validationResult);
        }

        protected IActionResult NotFoundError(string message)
        {
            return HandleError(new ServiceResult
            {
                Success = false,
                ErrorType = ServiceErrorType.NotFound,
                Errors = new List<string> { message },
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }
}
