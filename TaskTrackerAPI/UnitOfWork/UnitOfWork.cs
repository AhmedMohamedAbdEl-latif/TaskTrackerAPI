using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Data.SqlClient;
using System.Data;
using TaskTrackerAPI.Common;

namespace TaskTrackerAPI.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TaskTrackerContext _context;

        public UnitOfWork(TaskTrackerContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default)
        {
            return await _context.Database.BeginTransactionAsync(
                isolationLevel,
                cancellationToken);
        }

        public async Task AcquireApplicationLockAsync(
            string resource,
            CancellationToken cancellationToken = default)
        {
            var currentTransaction = _context.Database.CurrentTransaction
                ?? throw new InvalidOperationException(
                    "No active transaction found.");

            var connection = _context.Database.GetDbConnection();

            await using var command = connection.CreateCommand();

            command.Transaction = currentTransaction.GetDbTransaction();
            command.CommandText = "sp_getapplock";
            command.CommandType = CommandType.StoredProcedure;

            var returnParameter = new SqlParameter
            {
                ParameterName = "@RETURN_VALUE",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.ReturnValue
            };

            command.Parameters.Add(returnParameter);

            command.Parameters.Add(
                new SqlParameter("@Resource", SqlDbType.NVarChar, 255)
                {
                    Value = resource
                });

            command.Parameters.Add(new SqlParameter("@LockMode", "Exclusive"));
            command.Parameters.Add(new SqlParameter("@LockOwner", "Transaction"));

            command.Parameters.Add(
                new SqlParameter("@LockTimeout", SqlDbType.Int)
                {
                    Value = 5000
                });

            await command.ExecuteNonQueryAsync(cancellationToken);

            var result = Convert.ToInt32(returnParameter.Value);

            if (result < 0)
                throw new ApplicationLockException(result);
        }
    }
}