
namespace TaskTrackerAPI.Services
{
    public interface IUserService
    {
        Task<PagedResultDto<UserResponseDto>> ListAllAsync(
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default);

        Task<UserResponseDto?> FindByIDAsync(int id, CancellationToken cancellationToken = default);

        Task<ServiceResult<UserResponseDto>> UpdateAsync(
            int id,
            UserDto userDto,
            CancellationToken cancellationToken = default);

        Task<ServiceResult> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<PagedResultDto<UserResponseDto>> GetAllProjectedAsync(
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<UserResponseDto>> UpdateRoleAsync(
            int id,
            UpdateUserRoleDto roleDto,
            CancellationToken cancellationToken = default);
    }
}
