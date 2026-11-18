using TaskTrackerAPI.DTO.Pagination;

namespace TaskTrackerAPI.Repository
{
    public interface ITaskRepository
    {
        Task<List<ProjectTask>> ListAllAsync();
        Task<ProjectTask?> FindByIDAsync(int id);
        Task<List<ProjectTask>> GetByProjectIDAsync(int projectId);
        Task<List<ProjectTask>> GetByUserIDAsync(int userId);
        Task<List<ProjectTask>> GetByStatusAsync(ProjectTaskStatus status);
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

        Task<PagedResultDto<TaskResponseDto>> GetPagedAsync(int PageNumber, int PageSize);
        void Create(ProjectTask task);
        void Update(ProjectTask task);
        void Delete(ProjectTask task);
    }
}
