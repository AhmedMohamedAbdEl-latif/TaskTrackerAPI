namespace TaskTrackerAPI.Repository
{
    public interface IProjectRepository
    {
        Task<List<Project>> GetAllAsync();
        Task<Project?> FindByIDAsync(int id);
        Task<bool> ExistsAsync(int id);
        void Create(Project project);
        void Update(Project project);
        void Delete(int id);
    }
}
