using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace TaskTrackerAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly TaskTrackerContext context;
        private readonly AutoMapper.IConfigurationProvider _mapperConfig;

        public UserRepository(TaskTrackerContext _context, AutoMapper.IConfigurationProvider mapperConfig)
        {
            context = _context;
            _mapperConfig = mapperConfig;
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

        // Retrieves all entities from the database and maps them to DTOs in the Service layer using AutoMapper.
        public async Task<List<User>> ListAllAsync()
        {
         return await context.Users.ToListAsync();
        }

        public void Update(User user)
        {
            context.Users.Update(user);
        }

        // Retrieves data using AutoMapper's ProjectTo<TDto>() to project directly at the database level.
        public async Task<List<UserResponseDto>> GetAllProjectedAsync()
        {
            return await context.Users
                //.Select(user => new UserResponseDto
                //{
                //    Id = user.Id,
                //    Name = user.Name,
                //    Email = user.Email,
                //    Role = user.Role
                //})
                .ProjectTo<UserResponseDto>(_mapperConfig)
                .ToListAsync();
        }


    }
}
