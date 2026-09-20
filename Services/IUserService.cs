namespace TaskTrackerAPI.Services
{
    public interface IUserService
    {
        Task<List<User>> ListAllAsync();
        Task<User?> FindByIDAsync(int id);
        Task<ServiceResult<User>> CreateAsync(UserDto userDto);
        Task<ServiceResult<User>> UpdateAsync(int id, UserDto userDto);
        void Delete(int id);
    }
}
