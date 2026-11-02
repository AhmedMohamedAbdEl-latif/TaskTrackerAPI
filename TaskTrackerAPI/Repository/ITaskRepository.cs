namespace TaskTrackerAPI.Repository
{
    public interface ITaskRepository
    {
        Task<List<ProjectTask>> ListAllAsync();
        Task<ProjectTask?> FindByIDAsync(int id);
        List<ProjectTask?> GetByProjectID(int projectId);
        List<ProjectTask?> GetByUserID(int userId);
        List<ProjectTask?> GetByStatus(ProjectTaskStatus status);
        Task<bool> ExistsAsync(int id);
        Task<TaskStatsDto> GetStatsAsync();
        Task<List<TaskResponseDto>> GetAllProjectedAsync();
        Task<bool> CanUserAccessTaskAsync(int taskId, int userId);

        Task<List<ProjectTask>> GetAccessibleTasksByProjectAsync(
             int projectId,
             int userId);

        Task<List<ProjectTask>> GetAccessibleTasksByStatusAsync(
             ProjectTaskStatus status,
            int userId);
        Task<List<ProjectTask>> GetAccessibleTasksAsync(int userId);
        void Create(ProjectTask task);
        void Update(ProjectTask task);
        void Delete(int id);
    }
}
