using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Data;
using TaskTrackerAPI.DTO.Auth;

namespace TaskTrackerAPI.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly ILoginAttemptService _loginAttemptService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDataProtector _twoFactorProtector;
        public AuthService(
            IUserRepository userRepository,
            PasswordHasher<User> passwordHasher,
            IJwtService jwtService,
            ILoginAttemptService loginAttemptService,
            IUnitOfWork unitOfWork,
            IDataProtectionProvider dataProtectionProvider

            )
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _loginAttemptService = loginAttemptService;
            _unitOfWork = unitOfWork;
            _twoFactorProtector = dataProtectionProvider.CreateProtector(
                "TaskTrackerAPI.TwoFactorSecret.v1");
        }
        public async Task<ServiceResult<string>> LoginAsync(
            LoginDto loginDto,
            CancellationToken cancellationToken = default)
        {
            return await LoginAsync(loginDto, null, cancellationToken);
        }

        public async Task<ServiceResult<string>> LoginAsync(
            LoginDto loginDto,
            string? clientAddress,
            CancellationToken cancellationToken = default)
        {
            var validationResult = new ServiceResult<string>();

            var operationId = Guid.NewGuid();

            var normalizedEmail = loginDto.Email.Trim().ToLowerInvariant();
            // Account lockouts are isolated by source address so an attacker
            // cannot lock out a victim merely by targeting their email.
            // The value is hashed before persistence by LoginAttemptService.
            var lockoutIdentity = $"{normalizedEmail}\n{clientAddress?.Trim() ?? "unknown"}";

            // 1. Check whether the account is currently locked out.
            var lockoutResult = await _loginAttemptService.CheckLockoutAsync(
                lockoutIdentity,
                operationId,
                cancellationToken);

            if(lockoutResult?.Outcome == LoginAttemptOutcome.LockedOut)
            {
                validationResult.ErrorType = ServiceErrorType.Unauthorized;
                validationResult.Errors.Add("Too many failed login attempts. Please try again later.");

                return validationResult;
            }

            // Find the user
            var user = await _userRepository.FindByEmailAsync(normalizedEmail, cancellationToken);

            // Unknown email: record a failed attempt
            if(user == null)
            {
                await _loginAttemptService.RecordFailedAttemptAsync(
                    lockoutIdentity, operationId, cancellationToken);

                validationResult.ErrorType = ServiceErrorType.Unauthorized;
                validationResult.Errors.Add("Invalid email or password.");

                return validationResult;
            }

            // Verify the password
            var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

            if(passwordResult == PasswordVerificationResult.Failed)
            {
                await _loginAttemptService.RecordFailedAttemptAsync(lockoutIdentity, operationId, cancellationToken);

                validationResult.ErrorType = ServiceErrorType.Unauthorized;
                validationResult.Errors.Add("Invalid email or password.");

                return validationResult;
            }

            if (passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, loginDto.Password);
                _userRepository.Update(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            if (user.TwoFactorEnabled)
            {
                if (string.IsNullOrWhiteSpace(loginDto.TwoFactorCode))
                {
                    validationResult.ErrorType = ServiceErrorType.Unauthorized;
                    validationResult.Errors.Add("A two-factor authentication code is required.");
                    validationResult.RequiresTwoFactor = true;
                    return validationResult;
                }

                if (string.IsNullOrWhiteSpace(user.TwoFactorSecretProtected))
                {
                    validationResult.ErrorType = ServiceErrorType.InternalServerError;
                    validationResult.Errors.Add("Two-factor authentication is not configured correctly.");
                    return validationResult;
                }

                string secret;
                try
                {
                    secret = _twoFactorProtector.Unprotect(user.TwoFactorSecretProtected);
                }
                catch (CryptographicException)
                {
                    validationResult.ErrorType = ServiceErrorType.InternalServerError;
                    validationResult.Errors.Add("Two-factor authentication is not configured correctly.");
                    return validationResult;
                }

                var validTotp = TotpAuthenticator.TryVerify(
                    secret,
                    loginDto.TwoFactorCode,
                    DateTime.UtcNow,
                    out var matchedTimeStep);
                if (validTotp)
                {
                    validTotp = await _userRepository.TryMarkTwoFactorStepAsync(
                        user.Id,
                        matchedTimeStep,
                        cancellationToken);
                }
                var validRecoveryCode = !validTotp &&
                    loginDto.TwoFactorCode.Length == 32 &&
                    await TryConsumeRecoveryCodeAsync(
                        user.Id,
                        loginDto.TwoFactorCode,
                        cancellationToken);

                if (!validTotp && !validRecoveryCode)
                {
                    await _loginAttemptService.RecordFailedAttemptAsync(
                        lockoutIdentity, operationId, cancellationToken);
                    validationResult.ErrorType = ServiceErrorType.Unauthorized;
                    validationResult.Errors.Add("Invalid email, password, or authentication code.");
                    return validationResult;
                }
            }

            // Password is valid: reset attempts and record success.
            var resetResult = await _loginAttemptService.ResetAttemptsAsync(
                lockoutIdentity, operationId, cancellationToken);

            // A concurrent request may have activated lockout after the initial check.
            // Never issue a token if the serialized reset reports an active lockout.
            if (resetResult.Outcome == LoginAttemptOutcome.LockedOut)
            {
                validationResult.ErrorType = ServiceErrorType.Unauthorized;
                validationResult.Errors.Add("Too many failed login attempts. Please try again later.");
                return validationResult;
            }

            // Generate the JWT only after the successful reset is committed.
            var token = _jwtService.GenerateToken(user);

            validationResult.Success = true;
            validationResult.Data = token;

            return validationResult;
        }

        public async Task<ServiceResult<TwoFactorSetupDto>> SetupTwoFactorAsync(
            int userId,
            string password,
            CancellationToken cancellationToken = default)
        {
            var result = new ServiceResult<TwoFactorSetupDto>();
            var user = await _userRepository.FindByIDAsync(userId, cancellationToken);

            if (user is null)
            {
                result.ErrorType = ServiceErrorType.NotFound;
                result.Errors.Add("User not found.");
                return result;
            }

            if (user.TwoFactorEnabled)
            {
                result.ErrorType = ServiceErrorType.Conflict;
                result.Errors.Add("Two-factor authentication is already enabled.");
                return result;
            }

            var passwordCheck = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password);
            if (passwordCheck == PasswordVerificationResult.Failed)
            {
                result.ErrorType = ServiceErrorType.Unauthorized;
                result.Errors.Add("Password verification failed.");
                return result;
            }

            var secret = TotpAuthenticator.CreateSecret();
            user.TwoFactorSecretProtected = _twoFactorProtector.Protect(secret);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            result.Success = true;
            result.Data = new TwoFactorSetupDto
            {
                SharedKey = secret,
                AuthenticatorUri = TotpAuthenticator.CreateAuthenticatorUri(
                    secret,
                    user.Email,
                    "TaskTrackerAPI")
            };
            return result;
        }

        public async Task<ServiceResult<List<string>>> EnableTwoFactorAsync(
            int userId,
            string code,
            CancellationToken cancellationToken = default)
        {
            var result = new ServiceResult<List<string>>();
            await using var transaction = await _unitOfWork.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            await _unitOfWork.AcquireApplicationLockAsync(
                $"TwoFactorEnable:{userId}",
                cancellationToken);

            var user = await _userRepository.FindByIDAsync(userId, cancellationToken);

            if (user is null)
            {
                result.ErrorType = ServiceErrorType.NotFound;
                result.Errors.Add("User not found.");
                return result;
            }

            if (user.TwoFactorEnabled)
            {
                result.ErrorType = ServiceErrorType.Conflict;
                result.Errors.Add("Two-factor authentication is already enabled.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(user.TwoFactorSecretProtected))
            {
                result.ErrorType = ServiceErrorType.Validation;
                result.Errors.Add("Start two-factor setup before enabling it.");
                return result;
            }

            string secret;
            try
            {
                secret = _twoFactorProtector.Unprotect(user.TwoFactorSecretProtected);
            }
            catch (CryptographicException)
            {
                result.ErrorType = ServiceErrorType.InternalServerError;
                result.Errors.Add("Two-factor authentication setup has expired. Start setup again.");
                return result;
            }

            if (!TotpAuthenticator.TryVerify(
                    secret,
                    code,
                    DateTime.UtcNow,
                    out var matchedTimeStep))
            {
                result.ErrorType = ServiceErrorType.Validation;
                result.Errors.Add("The authentication code is invalid.");
                return result;
            }

            // Claim the TOTP step atomically so a concurrent enable/login
            // cannot reuse the same code more than once.
            if (!await _userRepository.TryMarkTwoFactorStepAsync(
                    user.Id,
                    matchedTimeStep,
                    cancellationToken))
            {
                result.ErrorType = ServiceErrorType.Validation;
                result.Errors.Add("The authentication code has already been used. Wait for a new code and try again.");
                return result;
            }

            user.TwoFactorEnabled = true;
            var recoveryCodes = Enumerable.Range(0, 10)
                .Select(_ => Convert.ToHexString(RandomNumberGenerator.GetBytes(16)))
                .ToList();
            user.TwoFactorRecoveryCodeHashesJson = JsonSerializer.Serialize(
                recoveryCodes.Select(HashRecoveryCode));
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            result.Success = true;
            result.Data = recoveryCodes;
            return result;
        }

        public async Task<ServiceResult> DisableTwoFactorAsync(
            int userId,
            string code,
            CancellationToken cancellationToken = default)
        {
            var result = new ServiceResult();
            var usedRecoveryCode = false;

            // Consume recovery codes under a database lock before attaching a
            // tracked user, so concurrent requests cannot reuse the same code.
            if (code.Length == 32)
            {
                usedRecoveryCode = await TryConsumeRecoveryCodeAsync(
                    userId,
                    code,
                    cancellationToken);
                if (!usedRecoveryCode)
                {
                    result.ErrorType = ServiceErrorType.Unauthorized;
                    result.Errors.Add("The authentication code is invalid or has already been used.");
                    return result;
                }
            }

            var user = await _userRepository.FindByIDAsync(userId, cancellationToken);

            if (user is null)
            {
                result.ErrorType = ServiceErrorType.NotFound;
                result.Errors.Add("User not found.");
                return result;
            }

            if (!user.TwoFactorEnabled || string.IsNullOrWhiteSpace(user.TwoFactorSecretProtected))
            {
                result.ErrorType = ServiceErrorType.Conflict;
                result.Errors.Add("Two-factor authentication is not enabled.");
                return result;
            }

            string secret;
            try
            {
                secret = _twoFactorProtector.Unprotect(user.TwoFactorSecretProtected);
            }
            catch (CryptographicException)
            {
                result.ErrorType = ServiceErrorType.InternalServerError;
                result.Errors.Add("Two-factor authentication is not configured correctly.");
                return result;
            }

            if (!usedRecoveryCode && !TotpAuthenticator.Verify(secret, code, DateTime.UtcNow))
            {
                result.ErrorType = ServiceErrorType.Unauthorized;
                result.Errors.Add("The authentication code is invalid.");
                return result;
            }

            user.TwoFactorEnabled = false;
            user.TwoFactorSecretProtected = null;
            user.TwoFactorRecoveryCodeHashesJson = null;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            result.Success = true;
            return result;
        }

        private async Task<bool> TryConsumeRecoveryCodeAsync(
            int userId,
            string code,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            await _unitOfWork.AcquireApplicationLockAsync(
                $"TwoFactorRecovery:{userId}",
                cancellationToken);

            // Reload the user after acquiring the lock so two simultaneous
            // logins cannot consume the same recovery code.
            var user = await _userRepository.FindByIDAsync(userId, cancellationToken);
            if (user is null || !user.TwoFactorEnabled ||
                string.IsNullOrWhiteSpace(user.TwoFactorRecoveryCodeHashesJson))
                return false;

            List<string> hashes;
            try
            {
                hashes = JsonSerializer.Deserialize<List<string>>(
                    user.TwoFactorRecoveryCodeHashesJson) ?? new List<string>();
            }
            catch (JsonException)
            {
                return false;
            }

            var submittedHash = HashRecoveryCode(code);
            var matchingIndex = hashes.FindIndex(hash =>
            {
                try
                {
                    return CryptographicOperations.FixedTimeEquals(
                        Convert.FromHexString(hash),
                        Convert.FromHexString(submittedHash));
                }
                catch (FormatException)
                {
                    return false;
                }
            });

            if (matchingIndex < 0)
                return false;

            hashes.RemoveAt(matchingIndex);
            user.TwoFactorRecoveryCodeHashesJson = JsonSerializer.Serialize(hashes);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }

        private static string HashRecoveryCode(string code)
        {
            var normalizedCode = code.Trim().ToUpperInvariant();
            return Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(normalizedCode)));
        }

        public async Task<ServiceResult> RegisterAsync(
    RegisterDto registerDto,
    CancellationToken cancellationToken = default)
        {
            var validationResult = new ServiceResult();

            if (string.IsNullOrWhiteSpace(registerDto.Email) ||
                registerDto.Email.Any(char.IsWhiteSpace))
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Email format is invalid.");
                return validationResult;
            }

            var normalizedEmail = registerDto.Email.Trim().ToLowerInvariant();

            var existingUser = await _userRepository.FindByEmailAsync(normalizedEmail, cancellationToken);

            if (existingUser != null)
            {
                validationResult.ErrorType = ServiceErrorType.Conflict;
                validationResult.Errors.Add("Email is already registered.");
                return validationResult;
            }

            var user = new User
            {
                Name = registerDto.Name.Trim(),
                Email = normalizedEmail,
                Role = UserRole.User
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                registerDto.Password);

            _userRepository.Create(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            validationResult.Success = true;

            return validationResult;
        }
    }
}
