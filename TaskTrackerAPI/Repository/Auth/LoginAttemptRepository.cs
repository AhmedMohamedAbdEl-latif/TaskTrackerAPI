using Microsoft.EntityFrameworkCore;

namespace TaskTrackerAPI.Repository.Auth
{
    public class LoginAttemptRepository : ILoginAttemptRepository
    {
        private readonly TaskTrackerContext _context;
        public LoginAttemptRepository(TaskTrackerContext context)
        {
            _context = context;
        }
        public void Create(LoginAttempt loginAttempt)
        {
            _context.LoginAttempts.Add(loginAttempt);
        }

        public void CreateOperation(LoginAttemptOperation operation)
        {
            _context.LoginAttemptOperations.Add(operation);
        }

        public async Task<LoginAttempt?> GetByEmailHashAsync(
            string emailHash,
            CancellationToken cancellationToken = default)
        {
            return await _context.LoginAttempts.
                FirstOrDefaultAsync(la => la.EmailHash == emailHash,
                cancellationToken);
        }

        public async Task<LoginAttemptOperation?> GetOperationByIdAsync(
            Guid operationId,
            CancellationToken cancellationToken = default)
        {
            return await _context.LoginAttemptOperations.
                FirstOrDefaultAsync(op => op.OperationId == operationId,
                cancellationToken);
        }
    }
}
