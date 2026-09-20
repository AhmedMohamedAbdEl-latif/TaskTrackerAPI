using Microsoft.EntityFrameworkCore;

namespace TaskTrackerAPI.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly TaskTrackerContext context;

        public ProjectRepository(TaskTrackerContext _context)
        {
            context = _context;
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

        public async Task<List<Project>> GetAllAsync()
        {
           return await context.Projects.Include(p => p.Owner).ToListAsync();
        }

        public void Update(Project project)
        {
            context.Projects.Update(project);
        }
    }
}
