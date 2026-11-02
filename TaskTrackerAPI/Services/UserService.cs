namespace TaskTrackerAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;
        public UserService
            (IUserRepository userRepository, 
            ICurrentUserService currentUser,
            IProjectRepository projectRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _currentUser = currentUser;
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public Task<ServiceResult<UserResponseDto>> CreateAsync(UserDto userDto)
        {
            var validationResult = new ServiceResult<UserResponseDto>();

            var user = new User
            {
                Name = userDto.Name,
                Email = userDto.Email,
                Role = userDto.Role,
            };

            _userRepository.Create(user);

            validationResult.Success = true;
            validationResult.Data = _mapper.Map<UserResponseDto>(user);

            return Task.FromResult(validationResult);

        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var result = new ServiceResult();

            var user = await _userRepository.FindByIDAsync(id);

            if (user == null)
            {
                result.ErrorType = ServiceErrorType.NotFound;
                result.Errors.Add("User not found.");
                return result;
            }

            var hasProjects = await _projectRepository.HasProjectsAsync(id);

            if (hasProjects)
            {
                result.ErrorType = ServiceErrorType.Conflict;
                result.Errors.Add(
                    "Cannot delete this user because they own one or more projects."
                );
                return result;
            }

            _userRepository.Delete(id);

            result.Success = true;
            return result;
        }

        public async Task<UserResponseDto?> FindByIDAsync(int id)
        {
            var user = await _userRepository.FindByIDAsync(id);

            return user != null ?_mapper.Map<UserResponseDto>(user) : null;
        }

        public async Task<List<UserResponseDto>> ListAllAsync()
        {
            var users = await _userRepository.ListAllAsync();

            return _mapper.Map<List<UserResponseDto>>(users);
        }

        public async Task<ServiceResult<UserResponseDto>> UpdateAsync(int id, UserDto userDto)
        {
          var validationResult = new ServiceResult<UserResponseDto>();
            var existingUser = await _userRepository.FindByIDAsync(id);

            if(existingUser == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("User not found.");
                return validationResult;
            }
            existingUser.Name = userDto.Name;
            existingUser.Email = userDto.Email;

            if (_currentUser.IsAdmin)
            {
                existingUser.Role = userDto.Role;
            }

            //  _userRepository.Update(existingUser);

            validationResult.Success = true;
            validationResult.Data = _mapper.Map<UserResponseDto>(existingUser);

            return validationResult;

        }

        public async Task<List<UserResponseDto>> GetAllProjectedAsync()
        {
            return await _userRepository.GetAllProjectedAsync();
        }
    }
}
