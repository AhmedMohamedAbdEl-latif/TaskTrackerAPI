namespace TaskTrackerAPI.Services
{
    public class ProjectService : IProjectService
    {
        // Dependency injection
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        // Constructor
        public ProjectService(
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }
        // Create a new project
        public async Task<ServiceResult<ProjectResponseDto>> CreateAsync(ProjectDto projectDto)
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

            var owner = await _userRepository.FindByIDAsync(projectDto.OwnerId);

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

            await _unitOfWork.SaveChangesAsync();

            validationResult.Success = true;
            validationResult.Data = _mapper.Map<ProjectResponseDto>(project);

            return validationResult;
        }
        // Delete a project
        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var validationResult = new ServiceResult();

            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            {
                validationResult.ErrorType = ServiceErrorType.Unauthorized;
                validationResult.Errors.Add("User is not authenticated.");
                return validationResult;
            }

            var project = await _projectRepository.FindByIDAsync(id);

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

            var hasTasks = await _projectRepository.HasTasksAsync(id);

            if (hasTasks)
            {
                validationResult.ErrorType = ServiceErrorType.Conflict;
                validationResult.Errors.Add(
                    "Cannot delete this project because it has associated tasks."
                );

                return validationResult;
            }

            _projectRepository.Delete(project);

            await _unitOfWork.SaveChangesAsync();

            validationResult.Success = true;

            return validationResult;
        }

        // Find a project by ID
        public async Task<ProjectResponseDto?> FindByIDAsync(int id)
        {
            var project = await _projectRepository.FindByIDAsync(id);

            if (project == null)
            {
                return null;
            }

            if (_currentUserService.IsAdmin)
            {
                return _mapper.Map<ProjectResponseDto>(project);
            }

            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                return null;
            }

            bool isOwner = project.OwnerId == userId.Value;

            bool isAssignedUser = await _projectRepository
                .HasTaskAssignedToUserAsync(project.Id, userId.Value);

            if (!isOwner && !isAssignedUser)
            {
                return null;
            }

            return _mapper.Map<ProjectResponseDto>(project);
        }

        // Get all projects
        public async Task<List<ProjectResponseDto>> GetAllAsync()
        {
            if (_currentUserService.IsAdmin)
            {
                var projects = await _projectRepository.GetAllAsync();

                return _mapper.Map<List<ProjectResponseDto>>(projects);
            }

            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                return new List<ProjectResponseDto>();
            }

            var accessibleProjects = await _projectRepository
                .GetAccessibleProjectsAsync(userId.Value);

            return _mapper.Map<List<ProjectResponseDto>>(accessibleProjects);
        }

        // Update a project
        public async Task<ServiceResult<ProjectResponseDto>> UpdateAsync(int id, ProjectDto projectDto)
        {
            var validationResult = new ServiceResult<ProjectResponseDto>();

            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            {
                validationResult.ErrorType = ServiceErrorType.Unauthorized;
                validationResult.Errors.Add("User is not authenticated.");
                return validationResult;
            }

            var project = await _projectRepository.FindByIDAsync(id);

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

            var owner = await _userRepository.FindByIDAsync(projectDto.OwnerId);

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

            await _unitOfWork.SaveChangesAsync();

            validationResult.Success = true;
            validationResult.Data = _mapper.Map<ProjectResponseDto>(project);

            return validationResult;
        }

        // Get all projected projects
        public async Task<List<ProjectResponseDto>> GetAllProjectedAsync()
        {
            return await _projectRepository.GetAllProjectedAsync();
        }
    }
}
