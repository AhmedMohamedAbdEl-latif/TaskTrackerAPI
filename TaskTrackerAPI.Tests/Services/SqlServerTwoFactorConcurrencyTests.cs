using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskTrackerAPI.Common;
using TaskTrackerAPI.DTO.Auth;
using TaskTrackerAPI.Models;
using TaskTrackerAPI.Repository;
using TaskTrackerAPI.Services.Auth;

namespace TaskTrackerAPI.Tests.Services;

/// <summary>
/// Real SQL Server tests for the database guarantees that mocked AuthService tests
/// cannot prove. Set TASKTRACKER_SQL_TEST_CONNECTION_STRING to a SQL Server
/// connection that can CREATE/DROP DATABASE before running these tests.
/// Each test creates and drops a uniquely named database; the supplied database
/// name is never opened or modified.
/// </summary>
public sealed class SqlServerTwoFactorConcurrencyTests : IAsyncLifetime
{
    private const string TotpSecret = "JBSWY3DPEHPK3PXPJBSWY3DPEHPK3PXP";
    private const string Password = "Correct-Horse-Battery-1";
    private SqlTestDatabase? _database;

    public SqlServerTwoFactorConcurrencyTests()
    {
    }

    public async Task InitializeAsync() => _database = await SqlTestDatabase.CreateInitializedAsync();

    public async Task DisposeAsync()
    {
        if (_database is not null)
            await _database.DisposeAsync();
    }

    [SqlServerFact]
    public async Task ConcurrentTotpLogins_OnlyOneRequestCanClaimTheSameTimeStep()
    {
        var database = _database!;
        var userId = await database.CreateUserAsync(user =>
        {
            user.TwoFactorEnabled = true;
            user.TwoFactorSecretProtected = database.Protector.Protect(TotpSecret);
        });
        var jwt = new Mock<IJwtService>();
        jwt.Setup(service => service.GenerateToken(It.IsAny<User>())).Returns("test-jwt");

        var code = Totp(DateTime.UtcNow);
        await using var firstContext = database.CreateContext();
        await using var secondContext = database.CreateContext();
        var results = await Task.WhenAll(
            database.CreateAuthService(firstContext, jwt.Object).LoginAsync(LoginWith(code)),
            database.CreateAuthService(secondContext, jwt.Object).LoginAsync(LoginWith(code)));

        Assert.Equal(1, results.Count(result => result.Success));
        Assert.Equal(1, results.Count(result => result.ErrorType == ServiceErrorType.Unauthorized));
        jwt.Verify(service => service.GenerateToken(It.IsAny<User>()), Times.Once);
        await using var verifyContext = database.CreateContext();
        Assert.NotNull((await verifyContext.Users.SingleAsync(user => user.Id == userId)).TwoFactorLastUsedTimeStep);
    }

    [SqlServerFact]
    public async Task ConcurrentRecoveryCodeLogins_OnlyOneRequestCanConsumeTheCode()
    {
        var database = _database!;
        const string recoveryCode = "FEDCBA9876543210FEDCBA9876543210";
        var userId = await database.CreateUserAsync(user =>
        {
            user.TwoFactorEnabled = true;
            user.TwoFactorSecretProtected = database.Protector.Protect(TotpSecret);
            user.TwoFactorRecoveryCodeHashesJson = JsonSerializer.Serialize(
                new[] { HashRecoveryCode(recoveryCode) });
        });
        var jwt = new Mock<IJwtService>();
        jwt.Setup(service => service.GenerateToken(It.IsAny<User>())).Returns("test-jwt");

        await using var firstContext = database.CreateContext();
        await using var secondContext = database.CreateContext();
        var results = await Task.WhenAll(
            database.CreateAuthService(firstContext, jwt.Object).LoginAsync(LoginWith(recoveryCode)),
            database.CreateAuthService(secondContext, jwt.Object).LoginAsync(LoginWith(recoveryCode)));

        Assert.Equal(1, results.Count(result => result.Success));
        Assert.Equal(1, results.Count(result => result.ErrorType == ServiceErrorType.Unauthorized));
        jwt.Verify(service => service.GenerateToken(It.IsAny<User>()), Times.Once);
        await using var verifyContext = database.CreateContext();
        var storedHashes = (await verifyContext.Users.SingleAsync(user => user.Id == userId))
            .TwoFactorRecoveryCodeHashesJson;
        Assert.Equal("[]", storedHashes);
    }

    [SqlServerFact]
    public async Task ConcurrentEnableRequests_OnlyOneRequestCanEnableTwoFactor()
    {
        var database = _database!;
        var userId = await database.CreateUserAsync(user =>
        {
            user.TwoFactorSecretProtected = database.Protector.Protect(TotpSecret);
        });
        var code = Totp(DateTime.UtcNow);

        await using var firstContext = database.CreateContext();
        await using var secondContext = database.CreateContext();
        var results = await Task.WhenAll(
            database.CreateAuthService(firstContext).EnableTwoFactorAsync(userId, code),
            database.CreateAuthService(secondContext).EnableTwoFactorAsync(userId, code));

        Assert.Single(results, result => result.Success);
        Assert.Single(results, result => result.ErrorType == ServiceErrorType.Conflict);
        await using var verifyContext = database.CreateContext();
        var user = await verifyContext.Users.SingleAsync(item => item.Id == userId);
        Assert.True(user.TwoFactorEnabled);
        Assert.NotNull(user.TwoFactorLastUsedTimeStep);
        Assert.Equal(10, JsonSerializer.Deserialize<List<string>>(
            user.TwoFactorRecoveryCodeHashesJson!)!.Count);
    }

    private static LoginDto LoginWith(string code) => new()
    {
        Email = "sql-concurrency@example.test",
        Password = Password,
        TwoFactorCode = code
    };

    private static string HashRecoveryCode(string code) => Convert.ToHexString(
        SHA256.HashData(Encoding.ASCII.GetBytes(code.Trim().ToUpperInvariant())));

    private static string Totp(DateTime utcNow)
    {
        var key = DecodeBase32(TotpSecret);
        var counterValue = new DateTimeOffset(utcNow).ToUnixTimeSeconds() / 30;
        Span<byte> counter = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(counter, counterValue);
        var digest = HMACSHA1.HashData(key, counter);
        var offset = digest[^1] & 0x0f;
        var binary = ((digest[offset] & 0x7f) << 24)
            | ((digest[offset + 1] & 0xff) << 16)
            | ((digest[offset + 2] & 0xff) << 8)
            | (digest[offset + 3] & 0xff);
        return (binary % 1_000_000).ToString("D6", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static byte[] DecodeBase32(string secret)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var bytes = new List<byte>();
        var buffer = 0;
        var bitsLeft = 0;
        foreach (var character in secret)
        {
            var value = alphabet.IndexOf(character);
            if (value < 0)
                throw new ArgumentException("Invalid test TOTP secret.", nameof(secret));
            buffer = (buffer << 5) | value;
            bitsLeft += 5;
            if (bitsLeft >= 8)
            {
                bytes.Add((byte)((buffer >> (bitsLeft - 8)) & 0xff));
                bitsLeft -= 8;
            }
        }
        return bytes.ToArray();
    }

    private sealed class SqlTestDatabase : IAsyncDisposable
    {
        private readonly string _databaseName = $"TaskTracker2FaTest_{Guid.NewGuid():N}";
        private readonly string _masterConnectionString;
        private readonly string _testConnectionString;
        private readonly PasswordHasher<User> _passwordHasher = new();
        private bool _databaseCreated;
        private readonly IDataProtectionProvider _dataProtectionProvider =
            new EphemeralDataProtectionProvider();

        public IDataProtector Protector => _dataProtectionProvider.CreateProtector(
            "TaskTrackerAPI.TwoFactorSecret.v1");

        public SqlTestDatabase()
        {
            var configuredConnectionString = Environment.GetEnvironmentVariable(
                "TASKTRACKER_SQL_TEST_CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(configuredConnectionString))
                throw new InvalidOperationException(
                    "Set TASKTRACKER_SQL_TEST_CONNECTION_STRING to a SQL Server connection with CREATE/DROP DATABASE permissions. These tests create isolated temporary databases and never touch your app database.");

            var builder = new SqlConnectionStringBuilder(configuredConnectionString);
            builder.InitialCatalog = "master";
            _masterConnectionString = builder.ConnectionString;
            builder.InitialCatalog = _databaseName;
            _testConnectionString = builder.ConnectionString;
        }

        public async Task<int> CreateUserAsync(Action<User> configure)
        {
            await using var context = CreateContext();
            var user = new User
            {
                Name = "Concurrency Test",
                Email = "sql-concurrency@example.test",
                Role = UserRole.User
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, Password);
            configure(user);
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user.Id;
        }

        public TaskTrackerContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TaskTrackerContext>()
                .UseSqlServer(_testConnectionString)
                .Options;
            return new TaskTrackerContext(options);
        }

        public AuthService CreateAuthService(TaskTrackerContext context, IJwtService? jwt = null)
        {
            var attempts = new Mock<ILoginAttemptService>();
            attempts.Setup(service => service.CheckLockoutAsync(
                    It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((LoginAttemptResult?)null);
            attempts.Setup(service => service.ResetAttemptsAsync(
                    It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LoginAttemptResult { Outcome = LoginAttemptOutcome.Succeeded });
            attempts.Setup(service => service.RecordFailedAttemptAsync(
                    It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LoginAttemptResult { Outcome = LoginAttemptOutcome.InvalidCredentials });

            return new AuthService(
                new UserRepository(context),
                _passwordHasher,
                jwt ?? Mock.Of<IJwtService>(),
                attempts.Object,
                new TaskTrackerAPI.UnitOfWork.UnitOfWork(context),
                _dataProtectionProvider);
        }

        private async Task CreateAsync()
        {
            await using (var connection = new SqlConnection(_masterConnectionString))
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = $"CREATE DATABASE [{_databaseName}]";
                await command.ExecuteNonQueryAsync();
                _databaseCreated = true;
            }

            await using var context = CreateContext();
            await context.Database.MigrateAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (!_databaseCreated)
                return;

            await using var connection = new SqlConnection(_masterConnectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_databaseName}]";
            await command.ExecuteNonQueryAsync();
            _databaseCreated = false;
        }

        public static async Task<SqlTestDatabase> CreateInitializedAsync()
        {
            var database = new SqlTestDatabase();
            try
            {
                await database.CreateAsync();
                return database;
            }
            catch
            {
                await database.DisposeAsync();
                throw;
            }
        }
    }
}

public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(
                "TASKTRACKER_SQL_TEST_CONNECTION_STRING")))
        {
            Skip = "Set TASKTRACKER_SQL_TEST_CONNECTION_STRING to run isolated SQL Server concurrency tests.";
        }
    }
}
