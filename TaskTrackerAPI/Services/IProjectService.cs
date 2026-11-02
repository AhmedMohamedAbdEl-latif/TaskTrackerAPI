namespace TaskTrackerAPI.Services
{
    public interface IProjectService
    {
        Task<List<ProjectResponseDto>> GetAllAsync();

        Task<ProjectResponseDto?> FindByIDAsync(int id);

        Task<ServiceResult<ProjectResponseDto>> CreateAsync(
            ProjectDto projectDto);

        Task<ServiceResult<ProjectResponseDto>> UpdateAsync(
            int id, ProjectDto projectDto);

        Task<ServiceResult> DeleteAsync(int id);

        Task<List<ProjectResponseDto>> GetAllProjectedAsync();
    }

}

