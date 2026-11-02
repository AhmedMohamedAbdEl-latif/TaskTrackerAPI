

namespace TaskTrackerAPI.Services
{
    public class TaskService : ITaskService
    {
        // Dependency Injection 
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        // Constructor
        public TaskService(
            ITaskRepository taskRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IMapper mapper
            )
        {
            _unitOfWork = unitOfWork;
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _currentUser = currentUser;
            _mapper = mapper;
        }
        // Create a new task
        public async Task<ServiceResult<TaskResponseDto>> CreateAsync(TaskDto taskDto)
        {
            var validationResult = new ServiceResult<TaskResponseDto>();

            var projectExists = await _projectRepository.ExistsAsync(taskDto.ProjectId);

            if (!projectExists)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Project not found");

                return validationResult;
            }

            var userExists = await _userRepository.ExistsAsync(taskDto.AssignedUserId);

            if (!userExists)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Assigned user not found");

                return validationResult;
            }

            if (taskDto.DueDate < DateTime.UtcNow)
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Due date cannot be in the past");

                return validationResult;
            }

            if (!Enum.IsDefined(typeof(ProjectTaskStatus), taskDto.Status))
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Invalid task status");

                return validationResult;
            }

            var task = new ProjectTask
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                DueDate = taskDto.DueDate,
                Status = taskDto.Status,
                ProjectId = taskDto.ProjectId,
                AssignedUserId = taskDto.AssignedUserId
            };

            _taskRepository.Create(task);

            await _unitOfWork.SaveChangesAsync();

            validationResult.Success = true;
            validationResult.Data = _mapper.Map<TaskResponseDto>(task);

            return validationResult;
        }

        // Delete a task 
        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var validationResult = new ServiceResult();

            var task = await _taskRepository.FindByIDAsync(id);

            if (task == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Task not found.");
                return validationResult;
            }

            if (!_currentUser.IsAdmin)
            {
                var userId = _currentUser.UserId;

                if (!userId.HasValue)
                {
                    validationResult.ErrorType = ServiceErrorType.Unauthorized;
                    validationResult.Errors.Add("User is not authenticated.");
                    return validationResult;
                }

                var isProjectOwner = task.Project.OwnerId == userId.Value;

                if (!isProjectOwner)
                {
                    validationResult.ErrorType = ServiceErrorType.Forbidden;
                    validationResult.Errors.Add("You are not allowed to delete this task.");
                    return validationResult;
                }
            }

            _taskRepository.Delete(id);

            await _unitOfWork.SaveChangesAsync();

            validationResult.Success = true;
            return validationResult;
        }

        // Find a task by ID
        public async Task<ServiceResult<TaskResponseDto>> FindByIDAsync(int id)
        {
            var result = new ServiceResult<TaskResponseDto>();

            var task = await _taskRepository.FindByIDAsync(id);

            if (task == null)
            {
                result.ErrorType = ServiceErrorType.NotFound;
                result.Errors.Add("Task not found.");
                return result;
            }

            if (!_currentUser.IsAdmin)
            {
                if (!_currentUser.UserId.HasValue)
                {
                    result.ErrorType = ServiceErrorType.Unauthorized;
                    result.Errors.Add("User is not authenticated.");
                    return result;
                }

                var canAccess = await _taskRepository.CanUserAccessTaskAsync(
                    id,
                    _currentUser.UserId.Value);

                if (!canAccess)
                {
                    result.ErrorType = ServiceErrorType.Forbidden;
                    result.Errors.Add("You are not allowed to view this task.");
                    return result;
                }
            }

            result.Success = true;
            result.Data = _mapper.Map<TaskResponseDto>(task);

            return result;
        }

        // Get tasks by project ID
        public async Task<ServiceResult<List<TaskResponseDto>>> GetByProjectID(int projectId)
        {
            var result = new ServiceResult<List<TaskResponseDto>>();

            if (_currentUser.IsAdmin)
            {
                var tasks = _taskRepository.GetByProjectID(projectId);
                result.Data = _mapper.Map<List<TaskResponseDto>>(tasks);
            }
            else
            {
                if (!_currentUser.UserId.HasValue)
                {
                    result.ErrorType = ServiceErrorType.Unauthorized;
                    result.Errors.Add("User is not authenticated.");
                    return result;
                }

                var tasks = await _taskRepository.GetAccessibleTasksByProjectAsync(
                    projectId,
                    _currentUser.UserId.Value);

                result.Data = _mapper.Map<List<TaskResponseDto>>(tasks);
            }

            result.Success = true;
            return result;
        }

        // Get tasks by status
        public async Task<ServiceResult<List<TaskResponseDto>>> GetByStatus(
              ProjectTaskStatus status)
        {
            var result = new ServiceResult<List<TaskResponseDto>>();

            if (!Enum.IsDefined(typeof(ProjectTaskStatus), status))
            {
                result.ErrorType = ServiceErrorType.Validation;
                result.Errors.Add("Invalid task status.");
                return result;
            }

            if (_currentUser.IsAdmin)
            {
                var tasks = _taskRepository.GetByStatus(status);
                result.Data = _mapper.Map<List<TaskResponseDto>>(tasks);
            }
            else
            {
                if (!_currentUser.UserId.HasValue)
                {
                    result.ErrorType = ServiceErrorType.Unauthorized;
                    result.Errors.Add("User is not authenticated.");
                    return result;
                }

                var tasks = await _taskRepository.GetAccessibleTasksByStatusAsync(
                    status,
                    _currentUser.UserId.Value);

                result.Data = _mapper.Map<List<TaskResponseDto>>(tasks);
            }

            result.Success = true;
            return result;
        }

        // Get tasks by user ID
        public ServiceResult<List<TaskResponseDto>> GetTasksByUserAsync(
            int requestedUserId)
        {
            var result = new ServiceResult<List<TaskResponseDto>>();

            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                result.ErrorType = ServiceErrorType.Unauthorized;
                result.Errors.Add("User is not authenticated.");
                return result;
            }

            if (!_currentUser.IsAdmin &&
                requestedUserId != _currentUser.UserId.Value)
            {
                result.ErrorType = ServiceErrorType.Forbidden;
                result.Errors.Add("You are not allowed to view these tasks.");
                return result;
            }

            var tasks = _taskRepository.GetByUserID(requestedUserId);

            result.Data = _mapper.Map<List<TaskResponseDto>>(tasks);

            result.Success = true;

            return result;
        }

        // Get task statistics
        public async Task<TaskStatsDto> GetStatsAsync()
        {
            return await _taskRepository.GetStatsAsync();
        }

        // List all tasks
        public async Task<ServiceResult<List<TaskResponseDto>>> ListAllAsync()
        {
            var result = new ServiceResult<List<TaskResponseDto>>();

            List<ProjectTask> tasks;

            if (_currentUser.IsAdmin)
            {
                tasks = await _taskRepository.ListAllAsync();
            }
            else
            {
                var userId = _currentUser.UserId;

                if (!_currentUser.IsAuthenticated || !userId.HasValue)
                {
                    result.ErrorType = ServiceErrorType.Unauthorized;
                    result.Errors.Add("User is not authenticated.");
                    return result;
                }

                tasks = await _taskRepository.GetAccessibleTasksAsync(userId.Value);
            }

            result.Data = _mapper.Map<List<TaskResponseDto>>(tasks);
            result.Success = true;

            return result;
        }

        // Update a task

        public async Task<ServiceResult> UpdateAsync(int id, TaskDto taskDto)
        {
            var validationResult = new ServiceResult();

            var existingTask = await _taskRepository.FindByIDAsync(id);

            if (existingTask == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Task not found.");
                return validationResult;
            }

            var userId = _currentUser.UserId;

            var isAssignedUser = false;
            var isProjectOwner = false;

            if (!_currentUser.IsAdmin)
            {
                if (!userId.HasValue)
                {
                    validationResult.ErrorType = ServiceErrorType.Unauthorized;
                    validationResult.Errors.Add("User is not authenticated.");
                    return validationResult;
                }

                isAssignedUser = existingTask.AssignedUserId == userId.Value;
                isProjectOwner = existingTask.Project.OwnerId == userId.Value;

                if (!isAssignedUser && !isProjectOwner)
                {
                    validationResult.ErrorType = ServiceErrorType.Forbidden;
                    validationResult.Errors.Add("You are not allowed to update this task.");
                    return validationResult;
                }
            }

            var destinationProject =
                await _projectRepository.FindByIDAsync(taskDto.ProjectId);

            if (destinationProject == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Project not found.");
                return validationResult;
            }

            if (!_currentUser.IsAdmin && !isProjectOwner &&
                taskDto.ProjectId != existingTask.ProjectId)
            {
                validationResult.ErrorType = ServiceErrorType.Forbidden;
                validationResult.Errors.Add("You are not allowed to move this task.");
                return validationResult;
            }

            if (!_currentUser.IsAdmin && isProjectOwner &&
                taskDto.ProjectId != existingTask.ProjectId &&
                destinationProject.OwnerId != userId.Value)
            {
                validationResult.ErrorType = ServiceErrorType.Forbidden;
                validationResult.Errors.Add("You can only move tasks to projects you own.");
                return validationResult;
            }

            var userExists = await _userRepository.ExistsAsync(taskDto.AssignedUserId);

            if (!userExists)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Assigned user not found.");
                return validationResult;
            }

            if (!_currentUser.IsAdmin && !isProjectOwner &&
                taskDto.AssignedUserId != existingTask.AssignedUserId)
            {
                validationResult.ErrorType = ServiceErrorType.Forbidden;
                validationResult.Errors.Add("You are not allowed to change the assigned user.");
                return validationResult;
            }

            if (taskDto.DueDate < DateTime.UtcNow)
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Due date cannot be in the past.");
                return validationResult;
            }

            if (!Enum.IsDefined(typeof(ProjectTaskStatus), taskDto.Status))
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Invalid task status.");
                return validationResult;
            }

            existingTask.Title = taskDto.Title;
            existingTask.Description = taskDto.Description;
            existingTask.DueDate = taskDto.DueDate;
            existingTask.Status = taskDto.Status;

            if (_currentUser.IsAdmin || isProjectOwner)
            {
                existingTask.ProjectId = taskDto.ProjectId;
            }

            if (_currentUser.IsAdmin || isProjectOwner)
            {
                existingTask.AssignedUserId = taskDto.AssignedUserId;
            }

            _taskRepository.Update(existingTask);

            await _unitOfWork.SaveChangesAsync();

            validationResult.Success = true;
            return validationResult;
        }

        public async Task<List<TaskResponseDto>> ListAllProjectedAsync()
        {   
           return await _taskRepository.GetAllProjectedAsync();
        }


    }
}
