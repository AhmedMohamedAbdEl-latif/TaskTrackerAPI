using Microsoft.EntityFrameworkCore;

namespace TaskTrackerAPI.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly TaskTrackerContext context;

        public CommentRepository(TaskTrackerContext _context)
        {
            context = _context;
        }
        public void Create(Comment comment)
        {
           context.Add(comment);
        }

        public void Delete(int id)
        {
            var comment = context.Comments.FirstOrDefault(c => c.Id == id);
            if (comment != null)
            {
                context.Comments.Remove(comment);
            }
        }

        public async Task<Comment> GetByIdAsync(int id)
        {
           return await context.Comments
                .Include(c => c.Task)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public List<Comment> GetByTaskId(int taskId)
        {
            return context.Comments
                .Include(c => c.Task)
                .ThenInclude(t => t.Project)
                .ThenInclude(p=> p.Owner)
                .Include(c => c.User)
                .Where(c => c.TaskId == taskId)
                .ToList();
        }

        public async Task<List<Comment>> ListAllAsync()
        {
           return await context.Comments
                .Include(c => c.Task)
                .Include(c => c.User)
                .ToListAsync();
        }

        public void Update(Comment comment)
        {
            context.Comments.Update(comment);
        }
    }
}
