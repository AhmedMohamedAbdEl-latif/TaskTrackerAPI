using TaskTrackerAPI.DTO.Auth;

namespace TaskTrackerAPI.Services.Auth
{
    public interface IAuthService
    {
        Task<ServiceResult> RegisterAsync(RegisterDto registerDto);
        Task<ServiceResult<string>> LoginAsync (LoginDto loginDto);
    }
}
