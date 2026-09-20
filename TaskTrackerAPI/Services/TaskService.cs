using TaskTrackerAPI.Common;

namespace TaskTrackerAPI.Services
{
    public class TaskService : ITaskService
    {
        // Dependency Injection 
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;

        // Constructor
        public TaskService(
            ITaskRepository taskRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository
            )
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
        }
        // Create a new task
        public async Task<ServiceResult<ProjectTask>> CreateAsync(TaskDto taskDto)
        {
            var validationResult = new ServiceResult<ProjectTask>();

            var projectExists = await _projectRepository.ExistsAsync(taskDto.ProjectId);

            if (!projectExists)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Project not found");
            }

            var userExists = await _userRepository.ExistsAsync(taskDto.AssignedUserId);

            if (!userExists)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Assigned user not found");
            }

            if (taskDto.DueDate < DateTime.UtcNow)
            {
                validationResult.Errors.Add("Due date cannot be in the past");

                if (validationResult.ErrorType == null)
                    validationResult.ErrorType = ServiceErrorType.Validation;
            }

            if (!Enum.IsDefined(typeof(ProjectTaskStatus), taskDto.Status))
                validationResult.Errors.Add("Invalid task status");

            if (validationResult.Errors.Any())
            {
                validationResult.Success = false;
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

            validationResult.Success = true;
            validationResult.Data = task;

            return validationResult;
        }
        // Delete a task 
        public void Delete(int id)
        {
            _taskRepository.Delete(id);
        }
        // Find a task by ID
        public async Task<ProjectTask?> FindByIDAsync(int id)
        {
            return await _taskRepository.FindByIDAsync(id);
        }
        // Get tasks by project ID
        public List<ProjectTask> GetByProjectID(int projectId)
        {
            return _taskRepository.GetByProjectID(projectId);
        }
        // Get tasks by status
        public List<ProjectTask> GetByStatus(ProjectTaskStatus status)
        {
            return _taskRepository.GetByStatus(status);
        }
        // Get tasks by user ID
        public List<ProjectTask> GetByUserID(int userId)
        {
            return _taskRepository.GetByUserID(userId);
        }

        public async Task<TaskStatsDto> GetStatsAsync()
        {
            return await _taskRepository.GetStatsAsync();
        }

        // List all tasks
        public async Task<List<ProjectTask>> ListAllAsync()
        {
            return await _taskRepository.ListAllAsync();
        }
        // Update a task
        public async Task<ServiceResult<ProjectTask>> UpdateAsync(
            int id, TaskDto taskDto,
            int currentUserId, bool isAdmin)
        {

            var validationResult = new ServiceResult<ProjectTask>();

            // 1. Get Task
            var existingTask = await _taskRepository.FindByIDAsync(id);

            if (existingTask == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Task not found.");
                return validationResult;
            }
            // 2. Check if User is allowed to update this Task
            if (!isAdmin && existingTask.AssignedUserId != currentUserId)
            {
                validationResult.ErrorType = ServiceErrorType.Forbidden;
                validationResult.Errors.Add("You are not allowed to update this task.");
                return validationResult;
            }
            // 3. Validate Project
            var projectExists = await _projectRepository.ExistsAsync(taskDto.ProjectId);

            if (!projectExists)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Project not found.");
            }
            // 4. Validate Assigned User
            var userExists = await _userRepository.ExistsAsync(taskDto.AssignedUserId);

            if (!userExists)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Assigned user not found.");
            }
            // 5. Validate DueDate
            if (taskDto.DueDate < DateTime.UtcNow)
            {
                validationResult.Errors.Add("Due date cannot be in the past.");

                if (validationResult.ErrorType == null)
                    validationResult.ErrorType = ServiceErrorType.Validation;
            }
            // 6. Validate Status
            if (!Enum.IsDefined(typeof(ProjectTaskStatus), taskDto.Status))
            {
                validationResult.Errors.Add("Invalid task status");

                if (validationResult.ErrorType == null)
                    validationResult.ErrorType = ServiceErrorType.Validation;
            }
            // 7. Check all validation errors
            if (validationResult.Errors.Any())
            {
                validationResult.Success = false;
                return validationResult;
            }
            // 8. User cannot change AssignedUser
            if (!isAdmin && taskDto.AssignedUserId != existingTask.AssignedUserId)
            {
                validationResult.ErrorType = ServiceErrorType.Forbidden;
                validationResult.Errors.Add(
                    "You are not allowed to change the assigned user.");

                return validationResult;
            }
            // 9. Update allowed fields
            existingTask.Title = taskDto.Title;
            existingTask.Description = taskDto.Description;
            existingTask.DueDate = taskDto.DueDate;
            existingTask.Status = taskDto.Status;
            existingTask.ProjectId = taskDto.ProjectId;
            // 10. Only Admin can change AssignedUser
            if (isAdmin)
            {
                existingTask.AssignedUserId = taskDto.AssignedUserId;
            }

            _taskRepository.Update(existingTask);

            validationResult.Success = true;
            validationResult.Data = existingTask;

            return validationResult;
        }

    }
}
