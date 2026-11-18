using Microsoft.EntityFrameworkCore.Storage;
using TaskTrackerAPI.Repository.Auth;

namespace TaskTrackerAPI.UnitOfWork.Auth
{
    public class LoginAttemptUnitOfWork : IAsyncDisposable
    {
        private readonly TaskTrackerContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private IDbContextTransaction? _transaction;
        public ILoginAttemptRepository Repository { get; }
        private bool _disposed;
        public LoginAttemptUnitOfWork(TaskTrackerContext context)
        {
            _context = context;
            _unitOfWork = new UnitOfWork(_context);
            Repository = new LoginAttemptRepository(_context);
        }

        public async Task BeginTransactionAsync(
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (_transaction != null)
                throw new InvalidOperationException("A transaction is already active.");

            _transaction = await _unitOfWork.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted,
                cancellationToken);
        }

        public Task AcquireApplicationLockAsync(
            string resource,
            CancellationToken cancellationToken = default)
        {
            EnsureActiveTransaction();

            return _unitOfWork.AcquireApplicationLockAsync
                (resource, cancellationToken);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            EnsureActiveTransaction();

            return _unitOfWork.SaveChangesAsync
                (cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            EnsureActiveTransaction();

            await _transaction!.CommitAsync(cancellationToken);
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (_transaction != null)
                await _transaction.RollbackAsync(cancellationToken);
        }
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                if (_transaction != null)
                    await _transaction.DisposeAsync();
            }
            finally
            {
                // The factory creates a dedicated DbContext for this unit of work.
                // It must be disposed whether transaction disposal succeeds or fails.
                await _context.DisposeAsync();
            }
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        private void EnsureActiveTransaction()
        {
            ThrowIfDisposed();

            if (_transaction == null)
                throw new InvalidOperationException("no active transaction found.");
        }
    }
}
