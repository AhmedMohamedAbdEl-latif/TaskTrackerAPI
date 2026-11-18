using Microsoft.AspNetCore.Identity;
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
        public AuthService(
            IUserRepository userRepository,
            PasswordHasher<User> passwordHasher,
            IJwtService jwtService,
            ILoginAttemptService loginAttemptService,
            IUnitOfWork unitOfWork

            )
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _loginAttemptService = loginAttemptService;
            _unitOfWork = unitOfWork;
        }
        public async Task<ServiceResult<string>> LoginAsync(
            LoginDto loginDto,
            CancellationToken cancellationToken = default)
        {
            var validationResult = new ServiceResult<string>();

            var operationId = Guid.NewGuid();

            var normalizedEmail = loginDto.Email.Trim().ToLowerInvariant();

            // 1. Check whether the account is currently locked out.
            var lockoutResult = await _loginAttemptService.CheckLockoutAsync(
                normalizedEmail,
                operationId,
                cancellationToken);

            if(lockoutResult?.Outcome == LoginAttemptOutcome.LockedOut)
            {
                validationResult.ErrorType = ServiceErrorType.Unauthorized;
                validationResult.Errors.Add("Too many failed login attempts. Please try again later.");

                return validationResult;
            }

            // Find the user
            var user = await _userRepository.FindByEmailAsync(normalizedEmail);

            // Unknown email: record a failed attempt
            if(user == null)
            {
                await _loginAttemptService.RecordFailedAttemptAsync(
                    normalizedEmail, operationId, cancellationToken);

                validationResult.ErrorType = ServiceErrorType.Unauthorized;
                validationResult.Errors.Add("Invalid email or password.");

                return validationResult;
            }

            // Verify the password
            var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

            if(passwordResult == PasswordVerificationResult.Failed)
            {
                await _loginAttemptService.RecordFailedAttemptAsync(normalizedEmail, operationId, cancellationToken);

                validationResult.ErrorType = ServiceErrorType.Unauthorized;
                validationResult.Errors.Add("Invalid email or password.");

                return validationResult;
            }

            // Password is valid: reset attempts and record success.
            var resetResult = await _loginAttemptService.ResetAttemptsAsync(
                normalizedEmail, operationId, cancellationToken);

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

            var existingUser = await _userRepository.FindByEmailAsync(normalizedEmail);

            if (existingUser != null)
            {
                validationResult.ErrorType = ServiceErrorType.Conflict;
                validationResult.Errors.Add("Email is already registered.");
                return validationResult;
            }

            var user = new User
            {
                Name = registerDto.Name,
                Email = normalizedEmail,
                Role = "User"
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
