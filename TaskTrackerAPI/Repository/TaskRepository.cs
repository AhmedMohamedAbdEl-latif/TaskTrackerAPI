using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TaskTrackerAPI.DTO.Pagination;

namespace TaskTrackerAPI.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly TaskTrackerContext _context;
        private readonly AutoMapper.IConfigurationProvider _mapperConfig;

        public TaskRepository(TaskTrackerContext context, AutoMapper.IConfigurationProvider mapperConfig)
        {
            _context = context;
            _mapperConfig = mapperConfig;
        }
        public void Create(ProjectTask task)
        {
            _context.Add(task);
        }
        public void Delete(ProjectTask task)
        {
            _context.Tasks.Remove(task);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Tasks.AnyAsync(t => t.Id == id);
        }

        public async Task<ProjectTask?> FindByIDAsync(int id)
        {
            return await _context.Tasks
                 .Include(t => t.Project)
                 .FirstOrDefaultAsync(t => t.Id == id);
        }
        public async Task<List<ProjectTask>> GetByProjectIDAsync(int projectId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.Id)
                .ToListAsync();
        }
        public async Task<List<ProjectTask>> GetByStatusAsync(ProjectTaskStatus status)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.Status == status)
                .OrderBy(t => t.Id)
                .ToListAsync();
        }

        public async Task<List<ProjectTask>> GetByUserIDAsync(int userId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.AssignedUserId == userId)
                .OrderBy(t => t.Id)
                .ToListAsync();
        }

        public async Task<TaskStatsDto> GetStatsAsync()
        {
            var now = DateTime.UtcNow;

            return new TaskStatsDto
            {
                ToDo = await _context.Tasks.CountAsync(t => t.Status == ProjectTaskStatus.ToDo),

                InProgress = await _context.Tasks.CountAsync(t => t.Status == ProjectTaskStatus.InProgress),

                Done = await _context.Tasks.CountAsync(t => t.Status == ProjectTaskStatus.Done),

                OverDue = await _context.Tasks.CountAsync(t => t.DueDate < now && t.Status != ProjectTaskStatus.Done),

            };

        }
        public async Task<List<ProjectTask>> ListAllAsync()
        {
            return await _context.Tasks
                .AsNoTracking()
                .OrderBy(t => t.Id)
                .ToListAsync();
        }
        public void Update(ProjectTask task)
        {
            _context.Tasks.Update(task);
        }

        // Retrieves data using AutoMapper's ProjectTo<TDto>() to project directly at the database level.
        public async Task<List<TaskResponseDto>> GetAllProjectedAsync()
        {
            return await _context.Tasks
                .OrderBy(t => t.Id)
                .ProjectTo<TaskResponseDto>(_mapperConfig)
                .ToListAsync();
        }
        public async Task<bool> CanUserAccessTaskAsync(int taskId, int userId)
        {
            return await _context.Tasks.AnyAsync(t =>
                t.Id == taskId &&
                (t.AssignedUserId == userId ||
                 t.Project.OwnerId == userId));
        }

        public async Task<List<ProjectTask>> GetAccessibleTasksByProjectAsync(
            int projectId,
            int userId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t =>
                    t.ProjectId == projectId &&
                    (t.AssignedUserId == userId ||
                     t.Project.OwnerId == userId))
                .OrderBy(t => t.Id)
                .ToListAsync();
        }

        public async Task<List<ProjectTask>> GetAccessibleTasksByStatusAsync(
            ProjectTaskStatus status,
            int userId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t =>
                    t.Status == status &&
                    (t.AssignedUserId == userId ||
                     t.Project.OwnerId == userId))
                .OrderBy(t => t.Id)
                .ToListAsync();
        }

        public async Task<List<ProjectTask>> GetAccessibleTasksAsync(int userId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t =>
                    t.AssignedUserId == userId ||
                    t.Project.OwnerId == userId)
                .OrderBy(t => t.Id)
                .ToListAsync();
        }

        public async Task<PagedResultDto<TaskResponseDto>> GetPagedAsync(
            int pageNumber,
            int pageSize)
        {
            var query = _context.Tasks.AsNoTracking();

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(t => t.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<TaskResponseDto>(_mapperConfig)
                .ToListAsync();

            return new PagedResultDto<TaskResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
