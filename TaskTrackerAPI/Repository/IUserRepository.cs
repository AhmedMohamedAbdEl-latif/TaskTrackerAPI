using TaskTrackerAPI.DTO.Pagination;

namespace TaskTrackerAPI.Repository
{
    public interface IUserRepository
    {
        Task<List<User>> ListAllAsync(CancellationToken cancellationToken = default);
        Task<User?> FindByIDAsync(int id, CancellationToken cancellationToken = default);
        Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<List<UserResponseDto>> GetAllProjectedAsync(CancellationToken cancellationToken = default);
        Task<PagedResultDto<UserResponseDto>> GetPagedAsync(
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default);
        Task<bool> HasAssignedTasksAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> HasCommentsAsync(int userId, CancellationToken cancellationToken = default);
        Task<int> CountAdminAsync(CancellationToken cancellationToken = default);
        Task<bool> TryMarkTwoFactorStepAsync(int userId, long timeStep, CancellationToken cancellationToken = default);
        void Update(User user);
        void Delete(User user);
        void Create(User user);
    }
}
