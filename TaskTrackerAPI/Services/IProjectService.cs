namespace TaskTrackerAPI.Services
{
    public interface IProjectService
    {
        Task<PagedResultDto<ProjectResponseDto>> GetAllAsync(
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default);

        Task<ProjectResponseDto?> FindByIDAsync(int id, CancellationToken cancellationToken = default);

        Task<ServiceResult<ProjectResponseDto>> CreateAsync(
            ProjectDto projectDto, CancellationToken cancellationToken = default);

        Task<ServiceResult<ProjectResponseDto>> UpdateAsync(
            int id, ProjectDto projectDto, CancellationToken cancellationToken = default);

        Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);

        Task<PagedResultDto<ProjectResponseDto>> GetAllProjectedAsync(
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default);
    }

}

