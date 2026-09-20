using Microsoft.EntityFrameworkCore;

namespace TaskTrackerAPI.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly TaskTrackerContext context;

        public TaskRepository(TaskTrackerContext _context)
        {
            context = _context;
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
                 .Include(t => t.AssignedUser)
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

        public async Task<List<ProjectTask>> ListAllAsync()
        {
            return await context.Tasks
                 .Include(t => t.Project)
                 .Include(t => t.AssignedUser)
                 .ToListAsync();
        }

        public void Update(ProjectTask task)
        {
            context.Tasks.Update(task);
        }
    }
}
