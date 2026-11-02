using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace TaskTrackerAPI.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly TaskTrackerContext context;
        private readonly AutoMapper.IConfigurationProvider _mapperConfig;

        public TaskRepository(TaskTrackerContext _context, AutoMapper.IConfigurationProvider mapperConfig)
        {
            context = _context;
            _mapperConfig = mapperConfig;               
        }
        public void Create(ProjectTask task)
        {
            context.Add(task);
        }
        public void Delete(int id)
        {
            var task = context.Tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
            {
                context.Tasks.Remove(task);
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await context.Tasks.AnyAsync(t => t.Id == id);
        }

        public async Task<ProjectTask?> FindByIDAsync(int id)
        {
            return await context.Tasks
                 .Include(t => t.Project)
                 .FirstOrDefaultAsync(t => t.Id == id);
        }
        public List<ProjectTask> GetByProjectID(int projectId)
        {
            return context.Tasks
                  .Where(t => t.ProjectId == projectId)
                  .Include(t => t.Project)
                  .Include(t => t.AssignedUser)
                  .ToList();
        }

        public List<ProjectTask> GetByStatus(ProjectTaskStatus status)
        {
            return context.Tasks
                 .Where(t => t.Status == status)
                 .Include(t => t.Project)
                 .Include(t => t.AssignedUser)
                 .ToList();
        }

        public List<ProjectTask> GetByUserID(int userId)
        {
            return context.Tasks
                .Where(t => t.AssignedUserId == userId)
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .ToList();
        }

        public async Task<TaskStatsDto> GetStatsAsync()
        {
            var now = DateTime.UtcNow;

            return new TaskStatsDto
            {
                ToDo = await context.Tasks.CountAsync(t => t.Status == ProjectTaskStatus.ToDo),

                InProgress = await context.Tasks.CountAsync(t => t.Status == ProjectTaskStatus.InProgress),

                Done = await context.Tasks.CountAsync(t => t.Status == ProjectTaskStatus.Done),

                OverDue = await context.Tasks.CountAsync(t => t.DueDate < now && t.Status != ProjectTaskStatus.Done),

            };

        }

        // Retrieves all entities from the database and maps them to DTOs in the Service layer using AutoMapper.
        public async Task<List<ProjectTask>> ListAllAsync()
        {
            return await context.Tasks.ToListAsync();
        }

        public void Update(ProjectTask task)
        {
            context.Tasks.Update(task);
        }

        // Retrieves data using AutoMapper's ProjectTo<TDto>() to project directly at the database level.
        public async Task<List<TaskResponseDto>> GetAllProjectedAsync()
        {
            return await context.Tasks
                //.Select(task => new TaskResponseDto
                //{
                //    Id = task.Id,
                //    Title = task.Title,
                //    Description = task.Description,
                //    DueDate = task.DueDate,
                //    Status = task.Status,
                //    ProjectId = task.ProjectId,
                //    AssignedUserId = task.AssignedUserId,
                //})
                .ProjectTo<TaskResponseDto>(_mapperConfig)
                .ToListAsync();
        }

        public async Task<bool> CanUserAccessTaskAsync(int taskId, int userId)
        {
            return await context.Tasks.AnyAsync(t =>
                t.Id == taskId &&
                (t.AssignedUserId == userId ||
                 t.Project.OwnerId == userId));
        }


        public async Task<List<ProjectTask>> GetAccessibleTasksByProjectAsync(
           int projectId,
           int userId)
        {
            return await context.Tasks
                .Where(t =>
                    t.ProjectId == projectId &&
                    (t.AssignedUserId == userId ||
                     t.Project.OwnerId == userId))
                .ToListAsync();
        }

        public async Task<List<ProjectTask>> GetAccessibleTasksByStatusAsync(
             ProjectTaskStatus status,
             int userId)
        {
            return await context.Tasks
                .Where(t =>
                    t.Status == status &&
                    (t.AssignedUserId == userId ||
                     t.Project.OwnerId == userId))
                .ToListAsync();
        }

        public async Task<List<ProjectTask>> GetAccessibleTasksAsync(int userId)
        {
            return await context.Tasks
                .Where(t =>
                    t.AssignedUserId == userId ||
                    t.Project.OwnerId == userId)
                .ToListAsync();
        }
    }
}
