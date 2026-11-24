using TaskTrackerAPI.Common;
using TaskTrackerAPI.DTO.Pagination;

namespace TaskTrackerAPI.Services
{
    public interface ITaskService
    {
        Task<ServiceResult<PagedResultDto<TaskResponseDto>>> ListAllAsync(
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default);
        Task<ServiceResult<TaskResponseDto>> FindByIDAsync(int id, CancellationToken cancellationToken = default);
        Task<ServiceResult<PagedResultDto<TaskResponseDto>>> GetByProjectID(
            int projectId,
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default);
        Task<ServiceResult<PagedResultDto<TaskResponseDto>>> GetTasksByUserAsync(
            int requestedUserId,
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default);
        Task<ServiceResult<PagedResultDto<TaskResponseDto>>> GetByStatus(
            ProjectTaskStatus status,
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default);
        Task<ServiceResult<TaskResponseDto>> CreateAsync(TaskDto taskDto, CancellationToken cancellationToken = default);
        Task<ServiceResult> UpdateAsync(int id, TaskDto taskDto, CancellationToken cancellationToken = default);
        Task<TaskStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
        Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedResultDto<TaskResponseDto>> ListAllProjectedAsync(
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default);
        Task<ServiceResult> UpdateStatusAsync(int id, UpdateTaskStatusDto statusDto, CancellationToken cancellationToken = default);
        Task<ServiceResult<PagedResultDto<TaskResponseDto>>> GetPagedAsync(PaginationQueryDto pagination, CancellationToken cancellationToken = default);
    }
}
