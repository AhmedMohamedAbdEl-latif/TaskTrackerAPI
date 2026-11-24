using Microsoft.EntityFrameworkCore;
using TaskTrackerAPI.DTO.Pagination;

namespace TaskTrackerAPI.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly TaskTrackerContext _context;

        public CommentRepository(TaskTrackerContext context)
        {
            _context = context;
        }
        public void Create(Comment comment)
        {
           _context.Add(comment);
        }

        public void Delete(Comment comment)
        {
            _context.Comments.Remove(comment);
        }

        public async Task<Comment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
           return await _context.Comments

                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<List<Comment>> GetByTaskIdAsync(int taskId, CancellationToken cancellationToken = default)
        {
            return await _context.Comments
                .AsNoTracking()
                .Where(c => c.TaskId == taskId)
                .OrderBy(c => c.Id)
                .ToListAsync(cancellationToken);
        }

        // Retrieves entities for mapping in the service layer.
        public async Task<List<Comment>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Comments
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .ToListAsync(cancellationToken);
        }
        public async Task<bool> HasCommentsByTaskIdAsync(int taskId, CancellationToken cancellationToken = default)
        {
            return await _context.Comments
                .AnyAsync(c => c.TaskId == taskId, cancellationToken);
        }

        public void Update(Comment comment)
        {
            _context.Comments.Update(comment);
        }

        // Project only the response fields in SQL.
        public async Task<List<CommentResponseDto>> GetAllProjectedAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Comments
                .OrderBy(c => c.Id)
                .Select(c => new CommentResponseDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    TaskId = c.TaskId,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResultDto<CommentResponseDto>> GetPagedAsync(
            PaginationQueryDto pagination,
            int? taskId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Comments.AsNoTracking();
            if (taskId.HasValue)
                query = query.Where(comment => comment.TaskId == taskId.Value);

            return await query
                .OrderBy(comment => comment.Id)
                .Select(comment => new CommentResponseDto
                {
                    Id = comment.Id,
                    UserId = comment.UserId,
                    TaskId = comment.TaskId,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt
                })
                .ToPagedResultAsync(pagination, cancellationToken);
        }
    }
}
