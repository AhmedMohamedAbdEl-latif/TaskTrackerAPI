
namespace TaskTrackerAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(
            IUserRepository userRepository,
            ICurrentUserService currentUser,
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _currentUser = currentUser;
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var validationResult = new ServiceResult();

            // Serialize admin removals/demotions so two concurrent requests
            // cannot both observe two admins and remove both.
            await using var transaction = await _unitOfWork.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable,
                cancellationToken);
            await _unitOfWork.AcquireApplicationLockAsync(
                "AdminMembership",
                cancellationToken);

            var user = await _userRepository.FindByIDAsync(
                id, cancellationToken);

            if (user == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("User not found.");
                return validationResult;
            }

            if (user.Role == UserRole.Admin)
            {
                var adminCount = await _userRepository.CountAdminAsync(
                    cancellationToken);

                if (adminCount <= 1)
                {
                    validationResult.ErrorType = ServiceErrorType.Conflict;
                    validationResult.Errors.Add(
                        "Cannot delete the last admin account.");

                    return validationResult;
                }
            }

            var hasProjects = await _projectRepository.HasProjectsAsync(
                id, cancellationToken);

            if (hasProjects)
            {
                validationResult.ErrorType = ServiceErrorType.Conflict;
                validationResult.Errors.Add(
                    "Cannot delete this user because they own one or more projects.");

                return validationResult;
            }

            var hasAssignedTasks = await _userRepository.HasAssignedTasksAsync(
                id, cancellationToken);

            var hasComments = await _userRepository.HasCommentsAsync(
                id, cancellationToken);

            if (hasAssignedTasks || hasComments)
            {
                validationResult.ErrorType = ServiceErrorType.Conflict;
                validationResult.Errors.Add(
                    "Cannot delete this user because they have assigned tasks or comments.");

                return validationResult;
            }

            _userRepository.Delete(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            validationResult.Success = true;
            return validationResult;
        }

        public async Task<UserResponseDto?> FindByIDAsync(int id, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.FindByIDAsync(id, cancellationToken);

            return user != null
                ? UserMapper.ToResponseDto(user)
                : null;
        }

        public async Task<PagedResultDto<UserResponseDto>> ListAllAsync(
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetPagedAsync(pagination, cancellationToken);
        }

        public async Task<ServiceResult<UserResponseDto>> UpdateAsync(
            int id,
            UserDto userDto,
            CancellationToken cancellationToken = default)
        {
            var validationResult = new ServiceResult<UserResponseDto>();

            var user = await _userRepository.FindByIDAsync(
                id, cancellationToken);

            if (user == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("User not found.");
                return validationResult;
            }

            if (string.IsNullOrWhiteSpace(userDto.Email) ||
                userDto.Email.Any(char.IsWhiteSpace))
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Email format is invalid.");
                return validationResult;
            }

            var normalizedEmail = userDto.Email.Trim().ToLowerInvariant();

            var userWithEmail = await _userRepository.FindByEmailAsync(
                normalizedEmail, cancellationToken);

            if (userWithEmail != null && userWithEmail.Id != id)
            {
                validationResult.ErrorType = ServiceErrorType.Conflict;
                validationResult.Errors.Add("Email is already registered.");
                return validationResult;
            }

            user.Name = userDto.Name.Trim();
            user.Email = normalizedEmail;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            validationResult.Success = true;
            validationResult.Data = UserMapper.ToResponseDto(user);

            return validationResult;
        }

        public async Task<ServiceResult<UserResponseDto>> UpdateRoleAsync(
            int id,
            UpdateUserRoleDto roleDto,
            CancellationToken cancellationToken = default)
        {
            var validationResult = new ServiceResult<UserResponseDto>();

            await using var transaction = await _unitOfWork.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable,
                cancellationToken);
            await _unitOfWork.AcquireApplicationLockAsync(
                "AdminMembership",
                cancellationToken);

            var user = await _userRepository.FindByIDAsync(
                id, cancellationToken);

            if (user == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("User not found.");
                return validationResult;
            }

            if (!Enum.IsDefined(roleDto.Role))
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Invalid role.");
                return validationResult;
            }

            var newRole = roleDto.Role;

            if (user.Role == UserRole.Admin && newRole == UserRole.User)
            {
                var adminCount = await _userRepository.CountAdminAsync(
                    cancellationToken);

                if (adminCount <= 1)
                {
                    validationResult.ErrorType = ServiceErrorType.Conflict;
                    validationResult.Errors.Add(
                        "Can't remove the role of the last admin.");

                    return validationResult;
                }
            }

            user.Role = newRole;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            validationResult.Success = true;
            validationResult.Data = UserMapper.ToResponseDto(user);

            return validationResult;
        }

        public async Task<PagedResultDto<UserResponseDto>> GetAllProjectedAsync(
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetPagedAsync(pagination, cancellationToken);
        }
    }
}
