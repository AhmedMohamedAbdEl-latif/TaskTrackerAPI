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
                ServiceErrorType.Forbidden => Forbid(),
                ServiceErrorType.Conflict => Conflict(result),
                _ => BadRequest(result)
            };
        }
    }
}
