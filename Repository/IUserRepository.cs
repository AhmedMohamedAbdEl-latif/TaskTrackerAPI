namespace TaskTrackerAPI.Repository
{
    public interface IUserRepository
    {
        Task<List<User>> ListAllAsync();
        Task<User?> FindByIDAsync(int id);
        Task<User?> FindByEmailAsync(string email);
        Task<bool> ExistsAsync(int id);
        void Update(User user);
        void Delete(int id);
        void Create(User user);
    }
}
