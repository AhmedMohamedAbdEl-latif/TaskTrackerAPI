
using Microsoft.EntityFrameworkCore;
using TaskTrackerAPI.DTO.Pagination;

namespace TaskTrackerAPI.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly TaskTrackerContext _context;

        public ProjectRepository(
            TaskTrackerContext context
)
        {
            _context = context;
        }

        public async Task<bool> HasTasksAsync(
            int projectId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Tasks.AnyAsync(
                t => t.ProjectId == projectId,
                cancellationToken);
        }

        public async Task<bool> HasProjectsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Projects.AnyAsync(
                p => p.OwnerId == userId,
                cancellationToken);
        }

        public void Create(Project project)
        {
            _context.Projects.Add(project);
        }

        public void Delete(Project project)
        {
            _context.Projects.Remove(project);
        }

        public async Task<bool> ExistsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Projects.AnyAsync(
                p => p.Id == id,
                cancellationToken);
        }

        public void Update(Project project)
        {
            _context.Projects.Update(project);
        }

        // Checks whether the user has a task assigned within the specified project.
        public async Task<bool> HasTaskAssignedToUserAsync(
            int projectId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Tasks.AnyAsync(
                t => t.ProjectId == projectId &&
                     t.AssignedUserId == userId,
                cancellationToken);
        }


        public async Task<Project?> FindByIDAsync(
              int id,
              CancellationToken cancellationToken = default)
        {
            return await _context.Projects
                .FirstOrDefaultAsync(
                    p => p.Id == id,
                    cancellationToken);
        }

        public async Task<List<Project>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Projects
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ProjectResponseDto>> GetAllProjectedAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Projects
                .OrderBy(p => p.Id)
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    OwnerId = p.OwnerId
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResultDto<ProjectResponseDto>> GetPagedAsync(
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default)
        {
            return await _context.Projects
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    OwnerId = p.OwnerId
                })
                .ToPagedResultAsync(pagination, cancellationToken);
        }

        public async Task<PagedResultDto<ProjectResponseDto>> GetAccessiblePagedAsync(
            int userId,
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default)
        {
            return await _context.Projects
                .AsNoTracking()
                .Where(p => p.OwnerId == userId || _context.Tasks.Any(t =>
                    t.ProjectId == p.Id && t.AssignedUserId == userId))
                .OrderBy(p => p.Id)
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    OwnerId = p.OwnerId
                })
                .ToPagedResultAsync(pagination, cancellationToken);
        }

        public async Task<List<Project>> GetAccessibleProjectsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Projects
                .AsNoTracking()
                .Where(p =>
                    p.OwnerId == userId ||
                    _context.Tasks.Any(t =>
                        t.ProjectId == p.Id &&
                        t.AssignedUserId == userId))
                .OrderBy(p => p.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
