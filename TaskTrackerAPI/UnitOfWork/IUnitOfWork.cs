using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace TaskTrackerAPI.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);

        Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default);

        Task AcquireApplicationLockAsync(
            string resource,
            CancellationToken cancellationToken = default);
    }
}