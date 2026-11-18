using Microsoft.EntityFrameworkCore;
using TaskTrackerAPI.UnitOfWork.Auth;

namespace TaskTrackerAPI.Services.Auth
{
    public class LoginAttemptUnitOfWorkFactory : ILoginAttemptUnitOfWorkFactory
    {
        private readonly IDbContextFactory<TaskTrackerContext> _contextFactory;
        public LoginAttemptUnitOfWorkFactory(IDbContextFactory<TaskTrackerContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<LoginAttemptUnitOfWork> CreateAsync(
            CancellationToken cancellationToken = default)
        {
            var context = await _contextFactory.CreateDbContextAsync
                (cancellationToken);
            try
            {
                return new LoginAttemptUnitOfWork(context);
            }
            catch
            {
                await context.DisposeAsync();
                throw;
            }
        }
    }
}
