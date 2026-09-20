namespace TaskTrackerAPI.Services
{
    public interface IProjectService
    {
        Task<List<Project>> GetAllAsync();
        Task<Project?> FindByIDAsync(int id);
        Task<ServiceResult<Project>> CreateAsync(ProjectDto projectDto);
        Task<ServiceResult<Project>> UpdateAsync(int id, ProjectDto projectDto);
        void Delete(int id);

    }
}
