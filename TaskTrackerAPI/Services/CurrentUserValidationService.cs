using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace TaskTrackerAPI.Services
{
    public class CurrentUserValidationService
        : ICurrentUserValidationService
    {
        private readonly TaskTrackerContext _context;

        public CurrentUserValidationService(TaskTrackerContext context)
        {
            _context = context;
        }

        public async Task<bool> ValidateAsync(
            ClaimsPrincipal principal,
            CancellationToken cancellationToken = default)
        {
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out var userId))
            {
                return false;
            }

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                return false;
            }

            if (!Enum.IsDefined(user.Role))
            {
                return false;
            }

            if (principal.Identity is not ClaimsIdentity identity)
            {
                return false;
            }

            var existingRoleClaims = identity
                .FindAll(ClaimTypes.Role)
                .ToList();

            foreach (var claim in existingRoleClaims)
            {
                identity.RemoveClaim(claim);
            }

            identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));

            return true;
        }
    }
}
