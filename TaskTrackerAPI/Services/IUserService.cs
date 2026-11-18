
namespace TaskTrackerAPI.Services
{
    public interface IUserService
    {
        Task<List<UserResponseDto>> ListAllAsync();

        Task<UserResponseDto?> FindByIDAsync(int id);

        Task<ServiceResult<UserResponseDto>> UpdateAsync(
            int id,
            UserDto userDto,
            CancellationToken cancellationToken = default);

        Task<ServiceResult> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<List<UserResponseDto>> GetAllProjectedAsync();

        Task<ServiceResult<UserResponseDto>> UpdateRoleAsync(
            int id,
            UpdateUserRoleDto roleDto,
            CancellationToken cancellationToken = default);
    }
}
