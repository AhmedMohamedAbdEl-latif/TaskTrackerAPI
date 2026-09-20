namespace TaskTrackerAPI.Services
{
    public class ProjectService : IProjectService
    {
        // Dependency injection 
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        // Constructor
        public ProjectService(IProjectRepository projectRepository, IUserRepository userRepository)
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
        }
        // Create a new project
        public async Task<ServiceResult<Project>> CreateAsync(ProjectDto projectDto)
        {
            var validationResult = new ServiceResult<Project>();

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

            validationResult.Success = true;
            validationResult.Data = project;

            return validationResult;
        }
        // Delete a project
        public void Delete(int id)
        {
            _projectRepository.Delete(id);
        }
        // Find a project by ID
        public async Task<Project> FindByIDAsync(int id)
        {
            return await _projectRepository.FindByIDAsync(id);
        }
        // Get all projects
        public async Task<List<Project>> GetAllAsync()
        {
            return await _projectRepository.GetAllAsync();
        }
        // Update a project
        public async Task<ServiceResult<Project>> UpdateAsync(int id, ProjectDto projectDto)
        {
            var validationResult = new ServiceResult<Project>();

            var existingProject = await _projectRepository.FindByIDAsync(id);

            if (existingProject == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Project not found.");
                return validationResult;
            }
            var owner = await _userRepository.FindByIDAsync(projectDto.OwnerId.Value);
            if (owner == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Owner not found.");
                return validationResult;
            }

            existingProject.Name = projectDto.Name;
            existingProject.OwnerId = owner.Id;

            _projectRepository.Update(existingProject);

            validationResult.Success = true;
            validationResult.Data = existingProject;

            return validationResult;
        }
    }
}
