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
        void Create(ProjectTask task);
        void Update(ProjectTask task);
        void Delete(int id);
    }
}
