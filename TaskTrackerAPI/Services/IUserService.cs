namespace TaskTrackerAPI.Services
{
    public interface IUserService
    {
        Task<List<UserResponseDto>> ListAllAsync();
        Task<UserResponseDto?> FindByIDAsync(int id);
        Task<ServiceResult<UserResponseDto>> CreateAsync(UserDto userDto);
        Task<ServiceResult<UserResponseDto>> UpdateAsync(int id, UserDto userDto);
        Task<ServiceResult> DeleteAsync(int id);
        Task<List<UserResponseDto>> GetAllProjectedAsync();
    }
}
