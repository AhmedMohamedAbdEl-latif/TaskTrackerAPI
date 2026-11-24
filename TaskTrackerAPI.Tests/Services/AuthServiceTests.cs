using System.Buffers.Binary;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using TaskTrackerAPI.Common;
using TaskTrackerAPI.DTO.Auth;
using TaskTrackerAPI.Models;
using TaskTrackerAPI.Repository;
using TaskTrackerAPI.Services.Auth;
using TaskTrackerAPI.UnitOfWork;

namespace TaskTrackerAPI.Tests.Services
{
    public class AuthServiceTests
    {
        private const string TotpSecret = "JBSWY3DPEHPK3PXPJBSWY3DPEHPK3PXP";
        private const string Password = "Correct-Horse-Battery-1";
        private const int UserId = 42;

        private readonly User _user;
        private readonly Mock<IUserRepository> _users = new();
        private readonly Mock<ILoginAttemptService> _loginAttempts = new();
        private readonly Mock<IJwtService> _jwt = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IDbContextTransaction> _transaction = new();
        private readonly PasswordHasher<User> _passwordHasher = new();
        private readonly IDataProtectionProvider _protectionProvider = new EphemeralDataProtectionProvider();
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _user = new User
            {
                Id = UserId,
                Name = "Test User",
                Email = "test@example.com",
                Role = UserRole.User
            };
            _user.PasswordHash = _passwordHasher.HashPassword(_user, Password);

            _users.Setup(repository => repository.FindByEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => _user);
            _users.Setup(repository => repository.FindByIDAsync(
                    UserId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => _user);
            _users.Setup(repository => repository.TryMarkTwoFactorStepAsync(
                    UserId,
                    It.IsAny<long>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _loginAttempts.Setup(service => service.CheckLockoutAsync(
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<LoginAttemptResult?>(null));
            _loginAttempts.Setup(service => service.RecordFailedAttemptAsync(
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LoginAttemptResult { Outcome = LoginAttemptOutcome.InvalidCredentials });
            _loginAttempts.Setup(service => service.ResetAttemptsAsync(
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LoginAttemptResult { Outcome = LoginAttemptOutcome.Succeeded });

            _jwt.Setup(service => service.GenerateToken(It.IsAny<User>()))
                .Returns("test-jwt");
            _unitOfWork.Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _unitOfWork.Setup(unit => unit.BeginTransactionAsync(
                    It.IsAny<IsolationLevel>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_transaction.Object);
            _unitOfWork.Setup(unit => unit.AcquireApplicationLockAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _transaction.Setup(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _service = new AuthService(
                _users.Object,
                _passwordHasher,
                _jwt.Object,
                _loginAttempts.Object,
                _unitOfWork.Object,
                _protectionProvider);
        }

        [Fact]
        public async Task Login_WithValidPasswordAndNoTwoFactor_ReturnsJwt()
        {
            var result = await _service.LoginAsync(new LoginDto
            {
                Email = _user.Email,
                Password = Password
            });

            Assert.True(result.Success);
            Assert.Equal("test-jwt", result.Data);
            _jwt.Verify(jwt => jwt.GenerateToken(_user), Times.Once);
            _loginAttempts.Verify(service => service.ResetAttemptsAsync(
                It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Login_WithTwoFactorEnabledAndNoCode_RequiresSecondFactorWithoutJwt()
        {
            EnableTestTotpOnUser();

            var result = await _service.LoginAsync(new LoginDto
            {
                Email = _user.Email,
                Password = Password
            });

            Assert.False(result.Success);
            Assert.True(result.RequiresTwoFactor);
            Assert.Equal(ServiceErrorType.Unauthorized, result.ErrorType);
            _jwt.Verify(jwt => jwt.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Login_WithInvalidTwoFactorCode_DoesNotIssueJwt()
        {
            EnableTestTotpOnUser();

            var result = await _service.LoginAsync(new LoginDto
            {
                Email = _user.Email,
                Password = Password,
                TwoFactorCode = "ABCDEF"
            });

            Assert.False(result.Success);
            Assert.Equal(ServiceErrorType.Unauthorized, result.ErrorType);
            _loginAttempts.Verify(service => service.RecordFailedAttemptAsync(
                It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
            _jwt.Verify(jwt => jwt.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Login_WithValidTotp_MarksStepAndIssuesJwtOnlyOnce()
        {
            EnableTestTotpOnUser();
            var code = GenerateTotp(TotpSecret, DateTime.UtcNow);

            var firstResult = await _service.LoginAsync(new LoginDto
            {
                Email = _user.Email,
                Password = Password,
                TwoFactorCode = code
            });

            Assert.True(firstResult.Success);
            _users.Verify(repository => repository.TryMarkTwoFactorStepAsync(
                UserId, It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Once);

            _users.Setup(repository => repository.TryMarkTwoFactorStepAsync(
                    UserId,
                    It.IsAny<long>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var replayResult = await _service.LoginAsync(new LoginDto
            {
                Email = _user.Email,
                Password = Password,
                TwoFactorCode = code
            });

            Assert.False(replayResult.Success);
            Assert.Equal(ServiceErrorType.Unauthorized, replayResult.ErrorType);
            _jwt.Verify(jwt => jwt.GenerateToken(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Login_WithRecoveryCode_ConsumesItAndRejectsReuse()
        {
            EnableTestTotpOnUser();
            const string recoveryCode = "0123456789ABCDEF0123456789ABCDEF";
            _user.TwoFactorRecoveryCodeHashesJson = JsonSerializer.Serialize(
                new[] { HashRecoveryCode(recoveryCode) });

            var firstResult = await _service.LoginAsync(new LoginDto
            {
                Email = _user.Email,
                Password = Password,
                TwoFactorCode = recoveryCode
            });

            Assert.True(firstResult.Success);
            Assert.Equal("[]", _user.TwoFactorRecoveryCodeHashesJson);

            var replayResult = await _service.LoginAsync(new LoginDto
            {
                Email = _user.Email,
                Password = Password,
                TwoFactorCode = recoveryCode
            });

            Assert.False(replayResult.Success);
            Assert.Equal(ServiceErrorType.Unauthorized, replayResult.ErrorType);
            _jwt.Verify(jwt => jwt.GenerateToken(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task SetupTwoFactor_WithWrongPasswordDoesNotCreateSecret()
        {
            var result = await _service.SetupTwoFactorAsync(UserId, "wrong-password");

            Assert.False(result.Success);
            Assert.Equal(ServiceErrorType.Unauthorized, result.ErrorType);
            Assert.Null(_user.TwoFactorSecretProtected);
            _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task SetupTwoFactor_WithCorrectPasswordReturnsUriAndStoresProtectedSecret()
        {
            var result = await _service.SetupTwoFactorAsync(UserId, Password);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.StartsWith("otpauth://totp/", result.Data!.AuthenticatorUri);
            Assert.NotEqual(result.Data.SharedKey, _user.TwoFactorSecretProtected);
            var protector = _protectionProvider.CreateProtector("TaskTrackerAPI.TwoFactorSecret.v1");
            Assert.Equal(result.Data.SharedKey, protector.Unprotect(_user.TwoFactorSecretProtected!));
        }

        [Fact]
        public async Task EnableTwoFactor_WhenAlreadyEnabledReturnsConflictWithoutChangingRecoveryCodes()
        {
            _user.TwoFactorEnabled = true;
            _user.TwoFactorRecoveryCodeHashesJson = "existing-hashes";

            var result = await _service.EnableTwoFactorAsync(UserId, "123456");

            Assert.False(result.Success);
            Assert.Equal(ServiceErrorType.Conflict, result.ErrorType);
            Assert.Equal("existing-hashes", _user.TwoFactorRecoveryCodeHashesJson);
            _users.Verify(repository => repository.TryMarkTwoFactorStepAsync(
                It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task EnableTwoFactor_ValidTotpStoresOnlyRecoveryCodeHashes()
        {
            _user.TwoFactorSecretProtected = CreateProtector().Protect(TotpSecret);
            var code = GenerateTotp(TotpSecret, DateTime.UtcNow);

            var result = await _service.EnableTwoFactorAsync(UserId, code);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(10, result.Data!.Count);
            Assert.All(result.Data, recoveryCode => Assert.Matches("^[A-F0-9]{32}$", recoveryCode));

            var storedHashes = JsonSerializer.Deserialize<List<string>>(
                _user.TwoFactorRecoveryCodeHashesJson!);
            Assert.NotNull(storedHashes);
            Assert.Equal(10, storedHashes!.Count);
            Assert.All(result.Data, recoveryCode =>
            {
                Assert.DoesNotContain(recoveryCode, _user.TwoFactorRecoveryCodeHashesJson!);
                Assert.Contains(HashRecoveryCode(recoveryCode), storedHashes);
            });
            Assert.True(_user.TwoFactorEnabled);
            _users.Verify(repository => repository.TryMarkTwoFactorStepAsync(
                UserId, It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Once);
            _transaction.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task EnableTwoFactor_RejectsAlreadyUsedTotpStep()
        {
            _user.TwoFactorSecretProtected = CreateProtector().Protect(TotpSecret);
            _users.Setup(repository => repository.TryMarkTwoFactorStepAsync(
                    UserId,
                    It.IsAny<long>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _service.EnableTwoFactorAsync(
                UserId,
                GenerateTotp(TotpSecret, DateTime.UtcNow));

            Assert.False(result.Success);
            Assert.Equal(ServiceErrorType.Validation, result.ErrorType);
            Assert.False(_user.TwoFactorEnabled);
            _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DisableTwoFactor_WithRecoveryCodeDisablesAndClearsSecrets()
        {
            EnableTestTotpOnUser();
            const string recoveryCode = "FEDCBA9876543210FEDCBA9876543210";
            _user.TwoFactorRecoveryCodeHashesJson = JsonSerializer.Serialize(
                new[] { HashRecoveryCode(recoveryCode) });

            var result = await _service.DisableTwoFactorAsync(UserId, recoveryCode);

            Assert.True(result.Success);
            Assert.False(_user.TwoFactorEnabled);
            Assert.Null(_user.TwoFactorSecretProtected);
            Assert.Null(_user.TwoFactorRecoveryCodeHashesJson);
            _transaction.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        private void EnableTestTotpOnUser()
        {
            _user.TwoFactorEnabled = true;
            _user.TwoFactorSecretProtected = CreateProtector().Protect(TotpSecret);
        }

        private IDataProtector CreateProtector()
        {
            return _protectionProvider.CreateProtector("TaskTrackerAPI.TwoFactorSecret.v1");
        }

        private static string HashRecoveryCode(string code)
        {
            return Convert.ToHexString(SHA256.HashData(
                Encoding.ASCII.GetBytes(code.Trim().ToUpperInvariant())));
        }

        private static string GenerateTotp(string secret, DateTime utcNow)
        {
            var key = DecodeBase32(secret);
            var timeStep = new DateTimeOffset(utcNow).ToUnixTimeSeconds() / 30;
            Span<byte> counter = stackalloc byte[sizeof(long)];
            BinaryPrimitives.WriteInt64BigEndian(counter, timeStep);
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
    }
}
