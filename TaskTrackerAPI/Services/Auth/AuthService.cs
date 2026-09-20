using Microsoft.AspNetCore.Identity;
using TaskTrackerAPI.DTO.Auth;

namespace TaskTrackerAPI.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IJwtService _jwtService;

        public AuthService(
            IUserRepository userRepository,
            PasswordHasher<User> passwordHasher,
            IJwtService jwtService
            
            )
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }
        public async Task<ServiceResult<string>> LoginAsync(LoginDto loginDto)
        {
            var validationResult = new ServiceResult<string>();

            var existingUser = await _userRepository.FindByEmailAsync(loginDto.Email);

            if (existingUser == null)
            {
                validationResult.Success = false;
                validationResult.Errors.Add("Invalid email or password");

                return validationResult;
            }

            var passwordResult = _passwordHasher.VerifyHashedPassword(
                existingUser,
                existingUser.PasswordHash,
                loginDto.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                validationResult.Success = false;
                validationResult.Errors.Add("Invalid email or password");

                return validationResult;
            }

            var token = _jwtService.GenerateToken(existingUser);

            validationResult.Success = true;
            validationResult.Data = token;

            return validationResult;
        }

        public async Task<ServiceResult> RegisterAsync(RegisterDto registerDto)
        {
            var validationResult = new ServiceResult();

            var existingUser = await _userRepository.FindByEmailAsync(registerDto.Email);

            if (existingUser != null)
            {
                validationResult.Success = false;
                validationResult.Errors.Add("Email is already registered");

                return validationResult;
            }

            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Role = "User"

            };

            user.PasswordHash = _passwordHasher.HashPassword(user, registerDto.Password);

            _userRepository.Create(user);

            validationResult.Success = true;

            return validationResult;
        }
    }
}
