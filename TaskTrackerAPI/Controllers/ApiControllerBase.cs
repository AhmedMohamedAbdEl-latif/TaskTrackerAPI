using Microsoft.AspNetCore.Mvc;

namespace TaskTrackerAPI.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult HandleError(ServiceResult result)
        {
            return result.ErrorType switch
            {
                ServiceErrorType.NotFound => NotFound(result),
                ServiceErrorType.Validation => BadRequest(result),
                ServiceErrorType.Unauthorized => Unauthorized(result),
                ServiceErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result),
                ServiceErrorType.Conflict => Conflict(result),
                _ => StatusCode(StatusCodes.Status500InternalServerError),
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
            return HandleError(validationResult);
        }
    }
}
