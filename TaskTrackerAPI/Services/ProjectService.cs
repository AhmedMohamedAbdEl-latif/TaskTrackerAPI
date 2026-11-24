namespace TaskTrackerAPI.Services
{
    public class ProjectService : IProjectService
    {
        // Dependency injection
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        // Constructor
        public ProjectService(
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }
        // Create a new project
        public async Task<ServiceResult<ProjectResponseDto>> CreateAsync(ProjectDto projectDto, CancellationToken cancellationToken = default)
        {
            var validationResult = new ServiceResult<ProjectResponseDto>();

            // Check if the user is authenticated and has the right permissions
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            {
                validationResult.ErrorType = ServiceErrorType.Unauthorized;
                validationResult.Errors.Add("User is not authenticated.");
                return validationResult;
            }

            var currentUserId = _currentUserService.UserId.Value;

            // If the user is not an admin, ensure they can only create projects for themselves
            if (!_currentUserService.IsAdmin && projectDto.OwnerId != currentUserId)
            {
                validationResult.ErrorType = ServiceErrorType.Forbidden;
                validationResult.Errors.Add("You can only create projects for yourself.");
                return validationResult;
            }

            var owner = await _userRepository.FindByIDAsync(projectDto.OwnerId, cancellationToken);

            // Check if the owner exists
            if (owner == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Owner not found.");
                return validationResult;
            }

            // Create the project
            var project = new Project
            {
                Name = projectDto.Name.Trim(),
                OwnerId = projectDto.OwnerId
            };

            // Save the project to the database
            _projectRepository.Create(project);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            validationResult.Success = true;
            validationResult.Data = ProjectMapper.ToResponseDto(project);

            return validationResult;
        }
        // Delete a project
        public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var validationResult = new ServiceResult();

            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            {
                validationResult.ErrorType = ServiceErrorType.Unauthorized;
                validationResult.Errors.Add("User is not authenticated.");
                return validationResult;
            }

            var project = await _projectRepository.FindByIDAsync(id, cancellationToken);

            if (project == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Project not found.");
                return validationResult;
            }

            var currentUserId = _currentUserService.UserId.Value;

            var isProjectOwner = project.OwnerId == currentUserId;

            if (!_currentUserService.IsAdmin && !isProjectOwner)
            {
                validationResult.ErrorType = ServiceErrorType.Forbidden;
                validationResult.Errors.Add("You are not allowed to delete this project.");
                return validationResult;
            }

            var hasTasks = await _projectRepository.HasTasksAsync(id, cancellationToken);

            if (hasTasks)
            {
                validationResult.ErrorType = ServiceErrorType.Conflict;
                validationResult.Errors.Add(
                    "Cannot delete this project because it has associated tasks."
                );

                return validationResult;
            }

            _projectRepository.Delete(project);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            validationResult.Success = true;

            return validationResult;
        }

        // Find a project by ID
        public async Task<ProjectResponseDto?> FindByIDAsync(int id, CancellationToken cancellationToken = default)
        {
            var project = await _projectRepository.FindByIDAsync(id, cancellationToken);

            if (project == null)
            {
                return null;
            }

            if (_currentUserService.IsAdmin)
            {
                return ProjectMapper.ToResponseDto(project);
            }

            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                return null;
            }

            bool isOwner = project.OwnerId == userId.Value;

            bool isAssignedUser = await _projectRepository
                .HasTaskAssignedToUserAsync(project.Id, userId.Value, cancellationToken);

            if (!isOwner && !isAssignedUser)
            {
                return null;
            }

            return ProjectMapper.ToResponseDto(project);
        }

        // Get all projects
        public async Task<PagedResultDto<ProjectResponseDto>> GetAllAsync(
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default)
        {
            if (_currentUserService.IsAdmin)
            {
                return await _projectRepository.GetPagedAsync(pagination, cancellationToken);
            }

            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                return new PagedResultDto<ProjectResponseDto>
                {
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize
                };
            }

            return await _projectRepository.GetAccessiblePagedAsync(
                userId.Value,
                pagination,
                cancellationToken);
        }

        // Update a project
        public async Task<ServiceResult<ProjectResponseDto>> UpdateAsync(int id, ProjectDto projectDto, CancellationToken cancellationToken = default)
        {
            var validationResult = new ServiceResult<ProjectResponseDto>();

            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            {
                validationResult.ErrorType = ServiceErrorType.Unauthorized;
                validationResult.Errors.Add("User is not authenticated.");
                return validationResult;
            }

            var project = await _projectRepository.FindByIDAsync(id, cancellationToken);

            if (project == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Project not found.");
                return validationResult;
            }

            var currentUserId = _currentUserService.UserId.Value;

            var isProjectOwner = project.OwnerId == currentUserId;

            if (!_currentUserService.IsAdmin && !isProjectOwner)
            {
                validationResult.ErrorType = ServiceErrorType.Forbidden;
                validationResult.Errors.Add("You are not allowed to update this project.");
                return validationResult;
            }

            if (!_currentUserService.IsAdmin && projectDto.OwnerId != project.OwnerId)
            {
                validationResult.ErrorType = ServiceErrorType.Forbidden;
                validationResult.Errors.Add("Only Admin can change project ownership.");
                return validationResult;
            }

            var owner = await _userRepository.FindByIDAsync(projectDto.OwnerId, cancellationToken);

            if (owner == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Owner not found.");
                return validationResult;
            }

            project.Name = projectDto.Name.Trim();

            if (_currentUserService.IsAdmin)
            {
                project.OwnerId = owner.Id;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            validationResult.Success = true;
            validationResult.Data = ProjectMapper.ToResponseDto(project);

            return validationResult;
        }

        // Get all projected projects
        public async Task<PagedResultDto<ProjectResponseDto>> GetAllProjectedAsync(
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default)
        {
            return await _projectRepository.GetPagedAsync(pagination, cancellationToken);
        }
    }
}
