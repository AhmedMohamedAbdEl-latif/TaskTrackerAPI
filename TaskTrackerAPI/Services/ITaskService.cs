using TaskTrackerAPI.Common;
using TaskTrackerAPI.DTO.Pagination;

namespace TaskTrackerAPI.Services
{
    public interface ITaskService
    {
        Task<ServiceResult<List<TaskResponseDto>>> ListAllAsync();
        Task<ServiceResult<TaskResponseDto>> FindByIDAsync(int id);
        Task<ServiceResult<List<TaskResponseDto>>> GetByProjectID(int projectId);
        Task<ServiceResult<List<TaskResponseDto>>> GetTasksByUserAsync(int requestedUserId);
        Task<ServiceResult<List<TaskResponseDto>>> GetByStatus(
             ProjectTaskStatus status);
        Task<ServiceResult<TaskResponseDto>> CreateAsync(TaskDto taskDto);
        Task<ServiceResult> UpdateAsync(int id, TaskDto taskDto);
        Task<TaskStatsDto> GetStatsAsync();
        Task<ServiceResult> DeleteAsync(int id);
        Task<List<TaskResponseDto>> ListAllProjectedAsync();
        Task<ServiceResult> UpdateStatusAsync(int id, UpdateTaskStatusDto statusDto);
        Task<ServiceResult<PagedResultDto<TaskResponseDto>>> GetPagedAsync(PaginationQueryDto pagination);
    }
}
