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
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

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

        [EnableRateLimiting("AuthPolicy")]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(loginDto, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(result);
        }
    }
}
