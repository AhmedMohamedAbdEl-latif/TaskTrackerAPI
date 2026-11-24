using Microsoft.EntityFrameworkCore;
using TaskTrackerAPI.DTO.Pagination;

namespace TaskTrackerAPI.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly TaskTrackerContext _context;

        public TaskRepository(TaskTrackerContext context)
        {
            _context = context;
        }
        public void Create(ProjectTask task)
        {
            _context.Add(task);
        }
        public void Delete(ProjectTask task)
        {
            _context.Tasks.Remove(task);
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Tasks.AnyAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<ProjectTask?> FindByIDAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Tasks
                 .Include(t => t.Project)
                 .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }
        public async Task<List<ProjectTask>> GetByProjectIDAsync(int projectId, CancellationToken cancellationToken = default)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.Id)
                .ToListAsync(cancellationToken);
        }
        public async Task<List<ProjectTask>> GetByStatusAsync(ProjectTaskStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.Status == status)
                .OrderBy(t => t.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ProjectTask>> GetByUserIDAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.AssignedUserId == userId)
                .OrderBy(t => t.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<TaskStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            return new TaskStatsDto
            {
                ToDo = await _context.Tasks.CountAsync(t => t.Status == ProjectTaskStatus.ToDo, cancellationToken),

                InProgress = await _context.Tasks.CountAsync(t => t.Status == ProjectTaskStatus.InProgress, cancellationToken),

                Done = await _context.Tasks.CountAsync(t => t.Status == ProjectTaskStatus.Done, cancellationToken),

                OverDue = await _context.Tasks.CountAsync(t => t.DueDate < now && t.Status != ProjectTaskStatus.Done, cancellationToken),

            };

        }
        public async Task<List<ProjectTask>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Tasks
                .AsNoTracking()
                .OrderBy(t => t.Id)
                .ToListAsync(cancellationToken);
        }
        public void Update(ProjectTask task)
        {
            _context.Tasks.Update(task);
        }

        // Project only the response fields in SQL.
        public async Task<List<TaskResponseDto>> GetAllProjectedAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Tasks
                .OrderBy(t => t.Id)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    Status = t.Status,
                    ProjectId = t.ProjectId,
                    AssignedUserId = t.AssignedUserId
                })
                .ToListAsync(cancellationToken);
        }
        public async Task<bool> CanUserAccessTaskAsync(int taskId, int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Tasks.AnyAsync(t =>
                t.Id == taskId &&
                (t.AssignedUserId == userId ||
                 t.Project.OwnerId == userId), cancellationToken);
        }

        public async Task<List<ProjectTask>> GetAccessibleTasksByProjectAsync(
            int projectId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t =>
                    t.ProjectId == projectId &&
                    (t.AssignedUserId == userId ||
                     t.Project.OwnerId == userId))
                .OrderBy(t => t.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ProjectTask>> GetAccessibleTasksByStatusAsync(
            ProjectTaskStatus status,
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t =>
                    t.Status == status &&
                    (t.AssignedUserId == userId ||
                     t.Project.OwnerId == userId))
                .OrderBy(t => t.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ProjectTask>> GetAccessibleTasksAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t =>
                    t.AssignedUserId == userId ||
                    t.Project.OwnerId == userId)
                .OrderBy(t => t.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResultDto<TaskResponseDto>> GetPagedAsync(
            PaginationQueryDto pagination,
            int? accessibleUserId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Tasks.AsNoTracking();
            if (accessibleUserId.HasValue)
            {
                var userId = accessibleUserId.Value;
                query = query.Where(t => t.AssignedUserId == userId ||
                    t.Project.OwnerId == userId);
            }

            return await ProjectResponse(query.OrderBy(t => t.Id))
                .ToPagedResultAsync(pagination, cancellationToken);
        }

        public async Task<PagedResultDto<TaskResponseDto>> GetPagedByProjectAsync(
            int projectId,
            PaginationQueryDto pagination,
            int? accessibleUserId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Tasks.AsNoTracking()
                .Where(t => t.ProjectId == projectId);
            if (accessibleUserId.HasValue)
            {
                var userId = accessibleUserId.Value;
                query = query.Where(t => t.AssignedUserId == userId ||
                    t.Project.OwnerId == userId);
            }

            return await ProjectResponse(query.OrderBy(t => t.Id))
                .ToPagedResultAsync(pagination, cancellationToken);
        }

        public async Task<PagedResultDto<TaskResponseDto>> GetPagedByStatusAsync(
            ProjectTaskStatus status,
            PaginationQueryDto pagination,
            int? accessibleUserId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Tasks.AsNoTracking()
                .Where(t => t.Status == status);
            if (accessibleUserId.HasValue)
            {
                var userId = accessibleUserId.Value;
                query = query.Where(t => t.AssignedUserId == userId ||
                    t.Project.OwnerId == userId);
            }

            return await ProjectResponse(query.OrderBy(t => t.Id))
                .ToPagedResultAsync(pagination, cancellationToken);
        }

        public async Task<PagedResultDto<TaskResponseDto>> GetPagedByUserAsync(
            int userId,
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default)
        {
            return await ProjectResponse(_context.Tasks.AsNoTracking()
                    .Where(t => t.AssignedUserId == userId)
                    .OrderBy(t => t.Id))
                .ToPagedResultAsync(pagination, cancellationToken);
        }

        private static IQueryable<TaskResponseDto> ProjectResponse(
            IQueryable<ProjectTask> query)
        {
            return query.Select(t => new TaskResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                DueDate = t.DueDate,
                Status = t.Status,
                ProjectId = t.ProjectId,
                AssignedUserId = t.AssignedUserId
            });
        }
    }
}
