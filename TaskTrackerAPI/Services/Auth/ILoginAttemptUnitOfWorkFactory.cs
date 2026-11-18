using TaskTrackerAPI.UnitOfWork.Auth;

namespace TaskTrackerAPI.Services.Auth
{
    public interface ILoginAttemptUnitOfWorkFactory
    {
        Task<LoginAttemptUnitOfWork> CreateAsync(CancellationToken cancellationToken = default);
    }
}
