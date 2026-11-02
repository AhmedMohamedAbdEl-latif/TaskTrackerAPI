namespace TaskTrackerAPI.Repository
{
    public interface IProjectRepository
    {
        Task<List<Project>> GetAllAsync();
        Task<Project?> FindByIDAsync(int id);
        Task<List<ProjectResponseDto>> GetAllProjectedAsync();
        Task<List<Project>> GetAccessibleProjectsAsync(int userId);
        Task<bool> HasTaskAssignedToUserAsync(int projectId, int userId);
        Task<bool> ExistsAsync(int id);
        Task<bool> HasTasksAsync(int projectId);
        Task<bool> HasProjectsAsync(int userId);
        void Create(Project project);
        void Update(Project project);
        void Delete(int id);
    }
}
