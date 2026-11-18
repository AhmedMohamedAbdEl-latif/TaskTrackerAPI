
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace TaskTrackerAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly TaskTrackerContext _context;
        private readonly AutoMapper.IConfigurationProvider _mapperConfig;

        public UserRepository(
            TaskTrackerContext context,
            AutoMapper.IConfigurationProvider mapperConfig)
        {
            _context = context;
            _mapperConfig = mapperConfig;
        }

        public void Create(User user)
        {
            _context.Users.Add(user);
        }

        public void Delete(User user)
        {
            _context.Users.Remove(user);
        }

        public async Task<bool> ExistsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users.AnyAsync(
                u => u.Id == id,
                cancellationToken);
        }

        public async Task<User?> FindByIDAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users.FirstOrDefaultAsync(
                u => u.Id == id,
                cancellationToken);
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
        }

        public async Task<User?> FindByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();

            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    u => u.Email == normalizedEmail,
                    cancellationToken);
        }

        public async Task<List<User>> ListAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<UserResponseDto>> GetAllProjectedAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .OrderBy(u => u.Id)
                .ProjectTo<UserResponseDto>(_mapperConfig)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> HasAssignedTasksAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Tasks.AnyAsync(
                t => t.AssignedUserId == userId,
                cancellationToken);
        }

        public async Task<bool> HasCommentsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Comments.AnyAsync(
                c => c.UserId == userId,
                cancellationToken);
        }

        public async Task<int> CountAdminAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Users.CountAsync(
                u => u.Role == "Admin",
                cancellationToken);
        }
    }
}
