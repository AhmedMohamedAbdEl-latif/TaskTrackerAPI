using TaskTrackerAPI.DTO.Pagination;

namespace TaskTrackerAPI.Repository
{
    public interface IProjectRepository
    {
        Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Project?> FindByIDAsync(int id, CancellationToken cancellationToken = default);
        Task<List<ProjectResponseDto>> GetAllProjectedAsync(CancellationToken cancellationToken = default);
        Task<PagedResultDto<ProjectResponseDto>> GetPagedAsync(
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default);
        Task<PagedResultDto<ProjectResponseDto>> GetAccessiblePagedAsync(
            int userId,
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default);
        Task<List<Project>> GetAccessibleProjectsAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> HasTaskAssignedToUserAsync(int projectId, int userId, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> HasTasksAsync(int projectId, CancellationToken cancellationToken = default);
        Task<bool> HasProjectsAsync(int userId, CancellationToken cancellationToken = default);
        void Create(Project project);
        void Update(Project project);
        void Delete(Project project);
    }
}
