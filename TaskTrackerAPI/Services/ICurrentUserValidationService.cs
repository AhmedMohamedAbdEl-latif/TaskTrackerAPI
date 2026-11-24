using System.Security.Claims;

namespace TaskTrackerAPI.Services
{
    public interface ICurrentUserValidationService
    {
        Task<bool> ValidateAsync(
            ClaimsPrincipal principal,
            CancellationToken cancellationToken = default);
    }
}
