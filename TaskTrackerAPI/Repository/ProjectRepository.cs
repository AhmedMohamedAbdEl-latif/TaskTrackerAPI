
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;

namespace TaskTrackerAPI.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly TaskTrackerContext _context;
        private readonly AutoMapper.IConfigurationProvider _mapperConfig;

        public ProjectRepository(
            TaskTrackerContext context,
            AutoMapper.IConfigurationProvider mapperConfig)
        {
            _context = context;
            _mapperConfig = mapperConfig;
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
                .ProjectTo<ProjectResponseDto>(_mapperConfig)
                .ToListAsync(cancellationToken);
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
