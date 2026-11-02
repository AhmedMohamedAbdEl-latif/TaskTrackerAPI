using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;

namespace TaskTrackerAPI.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly TaskTrackerContext context;
        private readonly AutoMapper.IConfigurationProvider _mapperConfig;

        public ProjectRepository(TaskTrackerContext _context, AutoMapper.IConfigurationProvider mapperConfig)
        {
            context = _context;
            _mapperConfig = mapperConfig;
        }

        public async Task<bool> HasTasksAsync(int projectId)
        {
            return await context.Tasks.AnyAsync(t => t.ProjectId == projectId);
        }
        public async Task<bool> HasProjectsAsync(int userId)
        {
            return await context.Projects.AnyAsync(p => p.OwnerId == userId);
        }

        public void Create(Project project)
        {
           context.Projects.Add(project);
        }

        public void Delete(int id)
        {
            var project = context.Projects.FirstOrDefault(p => p.Id == id);
            if (project != null)
            {
                context.Projects.Remove(project);
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await context.Projects.AnyAsync(p => p.Id == id);
        }

        public async Task<Project> FindByIDAsync(int id)
        {
            return await context.Projects.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id);
        }

        // Retrieves all entities from the database and maps them to DTOs in the Service layer using AutoMapper.
        public async Task<List<Project>> GetAllAsync()
        {
           return await context.Projects
                //.Include(p => p.Owner)
                .ToListAsync();
        }

        public void Update(Project project)
        {
            context.Projects.Update(project);
        }

        // Retrieves data using AutoMapper's ProjectTo<TDto>() to project directly at the database level.
        public async Task<List<ProjectResponseDto>> GetAllProjectedAsync()
        {
            return await context.Projects
                //.Select(project => new ProjectResponseDto 
                //   {
                //    Id = project.Id,
                //    Name = project.Name,
                //    OwnerId = project.OwnerId,

                //})
                .ProjectTo<ProjectResponseDto>(_mapperConfig)
                .ToListAsync();

        }
        // This method retrieves all projects that are either owned by the specified user or have tasks assigned to that user.
        public async Task<List<Project>> GetAccessibleProjectsAsync(int userId)
        {
            return await context.Projects
                .Where(p =>
                    p.OwnerId == userId ||
                    context.Tasks.Any(t =>
                        t.ProjectId == p.Id &&
                        t.AssignedUserId == userId))
                .ToListAsync();
        }
        // This method checks if a specific user has any tasks assigned within a given project.
        public async Task<bool> HasTaskAssignedToUserAsync(
              int projectId, int userId)
        {
            return await context.Tasks.AnyAsync(t =>
                t.ProjectId == projectId &&
                t.AssignedUserId == userId);
        }
    }
}
