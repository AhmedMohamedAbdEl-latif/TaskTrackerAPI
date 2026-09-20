using Microsoft.EntityFrameworkCore;

namespace TaskTrackerAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly TaskTrackerContext context;

        public UserRepository(TaskTrackerContext _context)
        {
            context = _context;
        }
        public void Create(User user)
        {
            context.Users.Add(user);
        }

        public void Delete(int id)
        {
            var user = context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                context.Users.Remove(user);
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await context.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<User?> FindByEmailAsync(string email)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> FindByIDAsync(int id)
        {
           return await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<User>> ListAllAsync()
        {
         return await context.Users.ToListAsync();
        }

        public void Update(User user)
        {
            context.Users.Update(user);
        }
        
    }
}
