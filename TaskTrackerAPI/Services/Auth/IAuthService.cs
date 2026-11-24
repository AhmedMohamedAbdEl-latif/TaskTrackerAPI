using TaskTrackerAPI.DTO.Auth;

namespace TaskTrackerAPI.Services.Auth
{
    public interface IAuthService
    {
        Task<ServiceResult> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken);

        Task<ServiceResult<string>> LoginAsync(
            LoginDto loginDto,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<string>> LoginAsync(
            LoginDto loginDto,
            string? clientAddress,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<TwoFactorSetupDto>> SetupTwoFactorAsync(
            int userId,
            string password,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<List<string>>> EnableTwoFactorAsync(
            int userId,
            string code,
            CancellationToken cancellationToken = default);

        Task<ServiceResult> DisableTwoFactorAsync(
            int userId,
            string code,
            CancellationToken cancellationToken = default);
    }
}
