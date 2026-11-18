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

        public void Delete(Comment comment)
        {
            _context.Comments.Remove(comment);
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
           return await _context.Comments

                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Comment>> GetByTaskIdAsync(int taskId)
        {
            return await _context.Comments
                .AsNoTracking()
                .Where(c => c.TaskId == taskId)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        // Retrieves all entities from the database and maps them to DTOs in the Service layer using AutoMapper.
        public async Task<List<Comment>> ListAllAsync()
        {
            return await _context.Comments
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .ToListAsync();
        }
        public async Task<bool> HasCommentsByTaskIdAsync(int taskId)
        {
            return await _context.Comments
                .AnyAsync(c => c.TaskId == taskId);
        }

        public void Update(Comment comment)
        {
            _context.Comments.Update(comment);
        }

        // Retrieves data using AutoMapper's ProjectTo<TDto>() to project directly at the database level.
        public async Task<List<CommentResponseDto>> GetAllProjectedAsync()
        {
            return await _context.Comments
                .OrderBy(c => c.Id)
                .ProjectTo<CommentResponseDto>(_mapperConfig)
                .ToListAsync();
        }
    }
}
