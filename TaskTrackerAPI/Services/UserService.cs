namespace TaskTrackerAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ServiceResult<User>> CreateAsync(UserDto userDto)
        {
            var validationResult = new ServiceResult<User>();

            var user = new User
            {
                Name = userDto.Name,
                Email = userDto.Email,
                Role = userDto.Role,
            };

            _userRepository.Create(user);

            validationResult.Success = true;
            validationResult.Data = user;

            return validationResult;
        }

        public void Delete(int id)
        {
            _userRepository.Delete(id);
        }

        public async Task<User> FindByIDAsync(int id)
        {
            return await _userRepository.FindByIDAsync(id);
        }

        public async Task<List<User>> ListAllAsync()
        {
            return await _userRepository.ListAllAsync();
        }

        public async Task<ServiceResult<User>> UpdateAsync(int id, UserDto userDto)
        {
          var validationResult = new ServiceResult<User>();
            var existingUser = await _userRepository.FindByIDAsync(id);

            if(existingUser == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("User not found.");
                return validationResult;
            }
            existingUser.Name = userDto.Name;
            existingUser.Email = userDto.Email;
            existingUser.Role = userDto.Role;

            _userRepository.Update(existingUser);

            validationResult.Success = true;
            validationResult.Data = existingUser;

            return validationResult;
        }
    }
}
