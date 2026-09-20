using TaskTrackerAPI.Common;

namespace TaskTrackerAPI.Services
{
    public interface ITaskService
    {
        Task<List<ProjectTask>> ListAllAsync();
        Task<ProjectTask?> FindByIDAsync(int id);
        List<ProjectTask?> GetByProjectID(int projectId);
        List<ProjectTask?> GetByUserID(int userId);
        List<ProjectTask?> GetByStatus(ProjectTaskStatus status);
        Task<ServiceResult<ProjectTask>> CreateAsync(TaskDto taskDto);
        Task<ServiceResult<ProjectTask>> UpdateAsync(int id, TaskDto taskDto, int currentUserId, bool isAdmin);
        Task<TaskStatsDto> GetStatsAsync();
        void Delete(int id);
    }
}
