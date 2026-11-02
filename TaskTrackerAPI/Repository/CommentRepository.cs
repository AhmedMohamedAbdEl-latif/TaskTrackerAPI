using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;

namespace TaskTrackerAPI.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly TaskTrackerContext _context;
        private readonly AutoMapper.IConfigurationProvider _mapperConfig;

        public CommentRepository(TaskTrackerContext context, AutoMapper.IConfigurationProvider mapperConfig)
        {
            _context = context;
            _mapperConfig = mapperConfig;
        }
        public void Create(Comment comment)
        {
           _context.Add(comment);
        }

        public void Delete(int id)
        {
            var comment = _context.Comments.FirstOrDefault(c => c.Id == id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
            }
        }

        public async Task<Comment> GetByIdAsync(int id)
        {
           return await _context.Comments
                .Include(c => c.Task)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public List<Comment> GetByTaskId(int taskId)
        {
            return _context.Comments
                .Include(c => c.Task)
                .ThenInclude(t => t.Project)
                .ThenInclude(p=> p.Owner)
                .Include(c => c.User)
                .Where(c => c.TaskId == taskId)
                .ToList();
        }
        // Retrieves all entities from the database and maps them to DTOs in the Service layer using AutoMapper.
        public async Task<List<Comment>> ListAllAsync()
        {
           return await _context.Comments
                //.Include(c => c.Task)
                //.Include(c => c.User)
                .ToListAsync();
        }

        public void Update(Comment comment)
        {
            _context.Comments.Update(comment);
        }

        // Retrieves data using AutoMapper's ProjectTo<TDto>() to project directly at the database level.
        public async Task<List<CommentResponseDto>> GetAllProjectedAsync()
        {
            return await _context.Comments
                //.Select(comment => new CommentResponseDto
                //{
                //    Id = comment.Id,
                //    UserId = comment.UserId,
                //    TaskId = comment.TaskId,
                //    Content = comment.Content,
                //    CreatedAt = comment.CreatedAt
                //})
                .ProjectTo<CommentResponseDto>(_mapperConfig)
                .ToListAsync();
        }
    }
}
