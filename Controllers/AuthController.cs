using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerAPI.DTO.Auth;
using TaskTrackerAPI.Services.Auth;

namespace TaskTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class AuthController : ApiControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _unitOfWork;

        public AuthController(IAuthService authService, IUnitOfWork unitOfWork)
        {
            _authService = authService;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);

            if (!result.Success)
                return HandleError(result);

            await _unitOfWork.SaveChangesAsync();

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success)
                return HandleError(result);

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-test")]
        public IActionResult AdminTest()
        {
            return Ok("You are an Admin");
        }
    }
}
