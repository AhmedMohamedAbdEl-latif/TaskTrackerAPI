namespace TaskTrackerAPI.Repository.Auth
{
    public interface ILoginAttemptRepository
    {
        Task<LoginAttempt?> GetByEmailHashAsync(string emailHash, CancellationToken cancellationToken = default);
        void Create(LoginAttempt loginAttempt);
        Task<LoginAttemptOperation?> GetOperationByIdAsync(Guid operationId, CancellationToken cancellationToken = default);
        void CreateOperation(LoginAttemptOperation operation);

    }
}
