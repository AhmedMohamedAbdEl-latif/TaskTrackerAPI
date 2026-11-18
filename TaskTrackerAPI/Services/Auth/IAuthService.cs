using TaskTrackerAPI.DTO.Auth;

namespace TaskTrackerAPI.Services.Auth
{
    public interface IAuthService
    {
        Task<ServiceResult> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken);

        Task<ServiceResult<string>> LoginAsync(
            LoginDto loginDto,
            CancellationToken cancellationToken = default);
    }
}
