using TaskTrackerAPI.DTO.Pagination;

namespace TaskTrackerAPI.Repository
{
    public interface ITaskRepository
    {
        Task<List<ProjectTask>> ListAllAsync(CancellationToken cancellationToken = default);
        Task<ProjectTask?> FindByIDAsync(int id, CancellationToken cancellationToken = default);
        Task<List<ProjectTask>> GetByProjectIDAsync(int projectId, CancellationToken cancellationToken = default);
        Task<List<ProjectTask>> GetByUserIDAsync(int userId, CancellationToken cancellationToken = default);
        Task<List<ProjectTask>> GetByStatusAsync(ProjectTaskStatus status, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<TaskStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
        Task<List<TaskResponseDto>> GetAllProjectedAsync(CancellationToken cancellationToken = default);
        Task<bool> CanUserAccessTaskAsync(int taskId, int userId, CancellationToken cancellationToken = default);

        Task<List<ProjectTask>> GetAccessibleTasksByProjectAsync(
             int projectId,
             int userId, CancellationToken cancellationToken = default);

        Task<List<ProjectTask>> GetAccessibleTasksByStatusAsync(
             ProjectTaskStatus status,
            int userId, CancellationToken cancellationToken = default);
        Task<List<ProjectTask>> GetAccessibleTasksAsync(int userId, CancellationToken cancellationToken = default);

        Task<PagedResultDto<TaskResponseDto>> GetPagedAsync(
            PaginationQueryDto pagination,
            int? accessibleUserId = null,
            CancellationToken cancellationToken = default);
        Task<PagedResultDto<TaskResponseDto>> GetPagedByProjectAsync(
            int projectId,
            PaginationQueryDto pagination,
            int? accessibleUserId = null,
            CancellationToken cancellationToken = default);
        Task<PagedResultDto<TaskResponseDto>> GetPagedByStatusAsync(
            ProjectTaskStatus status,
            PaginationQueryDto pagination,
            int? accessibleUserId = null,
            CancellationToken cancellationToken = default);
        Task<PagedResultDto<TaskResponseDto>> GetPagedByUserAsync(
            int userId,
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default);
        void Create(ProjectTask task);
        void Update(ProjectTask task);
        void Delete(ProjectTask task);
    }
}
