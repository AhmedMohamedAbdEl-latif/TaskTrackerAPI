using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskTrackerAPI.DTO.Auth;
using TaskTrackerAPI.Services.Auth;

namespace TaskTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ApiControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUser;
        public AuthController(
            IAuthService authService,
            ICurrentUserService currentUser)
        {
            _authService = authService;
            _currentUser = currentUser;
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthPolicy")]
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterDto registerDto,
            CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(
                 registerDto,
                 cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("two-factor/setup")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> SetupTwoFactor(
            TwoFactorSetupRequest request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.UserId.HasValue)
                return Unauthorized();

            var result = await _authService.SetupTwoFactorAsync(
                _currentUser.UserId.Value,
                request.Password,
                cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("two-factor/enable")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> EnableTwoFactor(
            TwoFactorCodeDto request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.UserId.HasValue)
                return Unauthorized();

            var result = await _authService.EnableTwoFactorAsync(
                _currentUser.UserId.Value,
                request.Code,
                cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("two-factor/disable")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> DisableTwoFactor(
            TwoFactorCodeDto request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.UserId.HasValue)
                return Unauthorized();

            var result = await _authService.DisableTwoFactorAsync(
                _currentUser.UserId.Value,
                request.Code,
                cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(result);
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthPolicy")]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto, CancellationToken cancellationToken)
        {
            var clientAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.LoginAsync(
                loginDto,
                clientAddress,
                cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(result);
        }
    }
}
