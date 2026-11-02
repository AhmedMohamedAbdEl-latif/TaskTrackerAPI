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

            if (string.IsNullOrWhiteSpace(projectDto.Name))
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Project name is required.");
                return validationResult;
            }

            if (!projectDto.OwnerId.HasValue || projectDto.OwnerId <= 0)
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("A valid Owner ID is required.");
                return validationResult;
            }

            var owner = await _userRepository.FindByIDAsync(projectDto.OwnerId.Value);

            if (owner == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Owner not found.");
                return validationResult;
            }


            var project = new Project
            {
                Name = projectDto.Name,
                OwnerId = projectDto.OwnerId.Value
            };

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

            var existingProject = await _projectRepository.FindByIDAsync(id);

            if (existingProject == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Project not found.");
                return validationResult;
            }

            bool hasTasks = await _projectRepository.HasTasksAsync(id);

            if (hasTasks)
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add(
                    "Cannot delete this project because it has associated tasks."
                );

                return validationResult;
            }

            _projectRepository.Delete(id);

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

            var existingProject = await _projectRepository.FindByIDAsync(id);

            if (existingProject == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Project not found.");
                return validationResult;
            }

            if (!projectDto.OwnerId.HasValue || projectDto.OwnerId <= 0)
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("A valid Owner ID is required.");
                return validationResult;
            }

            var owner = await _userRepository.FindByIDAsync(projectDto.OwnerId.Value);

            if (owner == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Owner not found.");
                return validationResult;
            }

            if (string.IsNullOrWhiteSpace(projectDto.Name))
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Project name is required.");
                return validationResult;
            }

            existingProject.Name = projectDto.Name;
            existingProject.OwnerId = owner.Id;

            // _projectRepository.Update(existingProject);

            await _unitOfWork.SaveChangesAsync();

            validationResult.Success = true;
            validationResult.Data = _mapper.Map<ProjectResponseDto>(existingProject);

            return validationResult;
        }

        // Get all projected projects
        public async Task<List<ProjectResponseDto>> GetAllProjectedAsync()
        {
            return await _projectRepository.GetAllProjectedAsync();
        }
    }
}
