
using Microsoft.EntityFrameworkCore;
using TaskTrackerAPI.DTO.Pagination;

namespace TaskTrackerAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly TaskTrackerContext _context;

        public UserRepository(
            TaskTrackerContext context
)
        {
            _context = context;
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
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResultDto<UserResponseDto>> GetPagedAsync(
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToPagedResultAsync(pagination, cancellationToken);
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
                u => u.Role == UserRole.Admin,
                cancellationToken);
        }

        public async Task<bool> TryMarkTwoFactorStepAsync(
            int userId,
            long timeStep,
            CancellationToken cancellationToken = default)
        {
            var updatedRows = await _context.Users
                .Where(u => u.Id == userId &&
                    (!u.TwoFactorLastUsedTimeStep.HasValue ||
                     u.TwoFactorLastUsedTimeStep.Value < timeStep))
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        u => u.TwoFactorLastUsedTimeStep,
                        (long?)timeStep),
                    cancellationToken);

            return updatedRows == 1;
        }
    }
}
