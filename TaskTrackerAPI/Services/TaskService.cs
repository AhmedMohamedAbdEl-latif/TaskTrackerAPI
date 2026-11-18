

using TaskTrackerAPI.DTO.Pagination;

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
        private readonly ICommentRepository _commentRepository;

        // Constructor
        public TaskService(
            ITaskRepository taskRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IMapper mapper,
            ICommentRepository commentRepository
            )
        {
            _unitOfWork = unitOfWork;
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _currentUser = currentUser;
            _mapper = mapper;
            _commentRepository = commentRepository;
        }
        // Create a new task
        public async Task<ServiceResult<TaskResponseDto>> CreateAsync(TaskDto taskDto)
        {
            var validationResult = new ServiceResult<TaskResponseDto>();

            if (!_currentUser.IsAuthenticated ||
                       !_currentUser.UserId.HasValue)
            {
                validationResult.ErrorType = ServiceErrorType.Unauthorized;
                validationResult.Errors.Add("User is not authenticated.");
                return validationResult;
            }

            if (taskDto.DueDate is not DateTime dueDate)
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Due date is required.");
                return validationResult;
            }

            if (taskDto.Status is not ProjectTaskStatus taskStatus ||
                !Enum.IsDefined(typeof(ProjectTaskStatus), taskStatus))
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Invalid task status.");
                return validationResult;
            }

            var project = await _projectRepository.FindByIDAsync(taskDto.ProjectId);

            if (project == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Project not found");

                return validationResult;
            }

            var currentUserId = _currentUser.UserId.Value;

            var isProjectOwner = project.OwnerId == currentUserId;

            if (!_currentUser.IsAdmin && !isProjectOwner)
            {
                validationResult.ErrorType = ServiceErrorType.Forbidden;
                validationResult.Errors.Add("You are not allowed to create tasks in this project.");

                return validationResult;
            }

            var assignedUserExists = await _userRepository.ExistsAsync(taskDto.AssignedUserId);

            if (!assignedUserExists)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Assigned user not found.");

                return validationResult;
            }

            if (dueDate < DateTime.UtcNow)
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Due date cannot be in the past.");

                return validationResult;
            }

            var task = new ProjectTask
            {
                Title = taskDto.Title.Trim(),
                Description = taskDto.Description,
                DueDate = dueDate,
                Status = taskStatus,
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

            // Authorization: only Admin or Project Owner can delete.
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
                    validationResult.Errors.Add(
                        "You are not allowed to delete this task.");
                    return validationResult;
                }
            }

            // Referential integrity: prevent deleting tasks with comments.
            var hasComments =
                await _commentRepository.HasCommentsByTaskIdAsync(id);

            if (hasComments)
            {
                validationResult.ErrorType = ServiceErrorType.Conflict;
                validationResult.Errors.Add(
                    "Task cannot be deleted because it has comments.");
                return validationResult;
            }

            _taskRepository.Delete(task);

            await _unitOfWork.SaveChangesAsync();

            validationResult.Success = true;
            return validationResult;
        }

        // Find a task by ID
        public async Task<ServiceResult<TaskResponseDto>> FindByIDAsync(int id)
        {
            var validationResult = new ServiceResult<TaskResponseDto>();

            var task = await _taskRepository.FindByIDAsync(id);

            if (task == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Task not found.");
                return validationResult;
            }

            if (!_currentUser.IsAdmin)
            {
                if (!_currentUser.UserId.HasValue)
                {
                    validationResult.ErrorType = ServiceErrorType.Unauthorized;
                    validationResult.Errors.Add("User is not authenticated.");
                    return validationResult;
                }

                var canAccess = await _taskRepository.CanUserAccessTaskAsync(
                    id,
                    _currentUser.UserId.Value);

                if (!canAccess)
                {
                    validationResult.ErrorType = ServiceErrorType.Forbidden;
                    validationResult.Errors.Add("You are not allowed to view this task.");
                    return validationResult;
                }
            }

            validationResult.Success = true;
            validationResult.Data = _mapper.Map<TaskResponseDto>(task);

            return validationResult;
        }

        // Get tasks by project ID
        public async Task<ServiceResult<List<TaskResponseDto>>> GetByProjectID(int projectId)
        {
            var validationResult = new ServiceResult<List<TaskResponseDto>>();

            var project = await _projectRepository.FindByIDAsync(projectId);

            if (project == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Project not found.");
                return validationResult;
            }

            if (!_currentUser.IsAdmin)
            {
                if (!_currentUser.UserId.HasValue)
                {
                    validationResult.ErrorType = ServiceErrorType.Unauthorized;
                    validationResult.Errors.Add("User is not authenticated.");
                    return validationResult;
                }

                var userId = _currentUser.UserId.Value;

                var isProjectOwner = project.OwnerId == userId;

                if (isProjectOwner)
                {
                    var tasks = await _taskRepository.GetByProjectIDAsync(projectId);
                    validationResult.Data = _mapper.Map<List<TaskResponseDto>>(tasks);
                }
                else
                {
                    var tasks = await _taskRepository.GetAccessibleTasksByProjectAsync(
                        projectId,
                        userId);

                    validationResult.Data = _mapper.Map<List<TaskResponseDto>>(tasks);
                }
            }
            else
            {
                var tasks = await _taskRepository.GetByProjectIDAsync(projectId);
                validationResult.Data = _mapper.Map<List<TaskResponseDto>>(tasks);
            }

            validationResult.Success = true;
            return validationResult;
        }

        // Get tasks by status
        public async Task<ServiceResult<List<TaskResponseDto>>> GetByStatus(
              ProjectTaskStatus status)
        {
            var validationResult = new ServiceResult<List<TaskResponseDto>>();

            if (!Enum.IsDefined(typeof(ProjectTaskStatus), status))
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Invalid task status.");
                return validationResult;
            }

            if (_currentUser.IsAdmin)
            {
                var tasks = await _taskRepository.GetByStatusAsync(status);
                validationResult.Data = _mapper.Map<List<TaskResponseDto>>(tasks);
            }
            else
            {
                if (!_currentUser.UserId.HasValue)
                {
                    validationResult.ErrorType = ServiceErrorType.Unauthorized;
                    validationResult.Errors.Add("User is not authenticated.");
                    return validationResult;
                }

                var tasks = await _taskRepository.GetAccessibleTasksByStatusAsync(
                    status,
                    _currentUser.UserId.Value);

                validationResult.Data = _mapper.Map<List<TaskResponseDto>>(tasks);
            }

            validationResult.Success = true;
            return validationResult;
        }

        // Get tasks by user ID
        public async Task<ServiceResult<List<TaskResponseDto>>> GetTasksByUserAsync(
            int requestedUserId)
        {
            var validationResult = new ServiceResult<List<TaskResponseDto>>();

            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                validationResult.ErrorType = ServiceErrorType.Unauthorized;
                validationResult.Errors.Add("User is not authenticated.");
                return validationResult;
            }

            if (!_currentUser.IsAdmin &&
                requestedUserId != _currentUser.UserId.Value)
            {
                validationResult.ErrorType = ServiceErrorType.Forbidden;
                validationResult.Errors.Add("You are not allowed to view these tasks.");
                return validationResult;
            }

            var tasks = await _taskRepository.GetByUserIDAsync(requestedUserId);

            validationResult.Data = _mapper.Map<List<TaskResponseDto>>(tasks);

            validationResult.Success = true;

            return validationResult;
        }

        // Get task statistics
        public async Task<TaskStatsDto> GetStatsAsync()
        {
            return await _taskRepository.GetStatsAsync();
        }

        // List all tasks
        public async Task<ServiceResult<List<TaskResponseDto>>> ListAllAsync()
        {
            var validationResult = new ServiceResult<List<TaskResponseDto>>();

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
                    validationResult.ErrorType = ServiceErrorType.Unauthorized;
                    validationResult.Errors.Add("User is not authenticated.");
                    return validationResult;
                }

                tasks = await _taskRepository.GetAccessibleTasksAsync(userId.Value);
            }

            validationResult.Data = _mapper.Map<List<TaskResponseDto>>(tasks);
            validationResult.Success = true;

            return validationResult;
        }

        // Update a task

        public async Task<ServiceResult> UpdateAsync(int id, TaskDto taskDto)
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
                if (!_currentUser.UserId.HasValue)
                {
                    validationResult.ErrorType = ServiceErrorType.Unauthorized;
                    validationResult.Errors.Add("User is not authenticated.");
                    return validationResult;
                }

                validationResult.ErrorType = ServiceErrorType.Forbidden;
                validationResult.Errors.Add(
                    "Only admin can perform a full task update." + "Assignees can update task status only"
                    );
                return validationResult;
            }

            if (taskDto.DueDate is not DateTime dueDate)
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Due date is required.");
                return validationResult;
            }

            if (taskDto.Status is not ProjectTaskStatus taskStatus ||
                !Enum.IsDefined(typeof(ProjectTaskStatus), taskStatus))
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Invalid task status.");
                return validationResult;
            }

            var destinationProject =
                await _projectRepository.FindByIDAsync(taskDto.ProjectId);

            if (destinationProject == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Project not found.");
                return validationResult;
            }

            var userExists = await _userRepository.ExistsAsync(taskDto.AssignedUserId);

            if (!userExists)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Assigned user not found.");
                return validationResult;
            }

            if (dueDate != task.DueDate &&
                dueDate < DateTime.UtcNow)
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Due date cannot be in the past.");

                return validationResult;
            }

            if (!IsValidStatusTransition(
                  task.Status,
                  taskStatus
                ))
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add($"can not change task status from {task.Status} to {taskStatus}.");
                return validationResult;
            }

            task.Title = taskDto.Title.Trim();
            task.Description = taskDto.Description;
            task.DueDate = dueDate;
            task.Status = taskStatus;
            task.ProjectId = taskDto.ProjectId;
            task.AssignedUserId = taskDto.AssignedUserId;

            await _unitOfWork.SaveChangesAsync();

            validationResult.Success = true;
            return validationResult;
        }

        // List all tasks with projected data
        public async Task<List<TaskResponseDto>> ListAllProjectedAsync()
        {
            return await _taskRepository.GetAllProjectedAsync();
        }

        // Update the status of a task
        public async Task<ServiceResult> UpdateStatusAsync(int id, UpdateTaskStatusDto statusDto)
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
                if (!_currentUser.UserId.HasValue)
                {
                    validationResult.ErrorType = ServiceErrorType.Unauthorized;
                    validationResult.Errors.Add("User is not authenticated.");
                    return validationResult;
                }

                if (task.AssignedUserId != _currentUser.UserId.Value)
                {
                    validationResult.ErrorType = ServiceErrorType.Forbidden;
                    validationResult.Errors.Add("You are not allowed to update the status of this task.");
                    return validationResult;
                }
            }


            if (statusDto.Status is not ProjectTaskStatus newStatus)
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Status is required.");
                return validationResult;
            }

            if (!Enum.IsDefined(typeof(ProjectTaskStatus), statusDto.Status.Value))
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Invalid task status.");
                return validationResult;
            }

            if (!IsValidStatusTransition(
                task.Status,
                newStatus
                ))
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add($"can not change task status from {task.Status} to {newStatus}.");
                return validationResult;
            }

            if (task.Status == newStatus)
            {
                validationResult.Success = true;

                return validationResult;
            }

            task.Status = newStatus;

            await _unitOfWork.SaveChangesAsync();

            validationResult.Success = true;

            return validationResult;
        }

        public static bool IsValidStatusTransition(
            ProjectTaskStatus currentStatus,
            ProjectTaskStatus newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                _ when currentStatus == newStatus => true,

                (ProjectTaskStatus.ToDo,
                ProjectTaskStatus.InProgress) => true,

                (ProjectTaskStatus.InProgress,
                ProjectTaskStatus.Done) => true,

                _ => false
            };

        }

        public async Task<ServiceResult<PagedResultDto<TaskResponseDto>>> GetPagedAsync(PaginationQueryDto pagination)
        {
            var validationResult = new ServiceResult<PagedResultDto<TaskResponseDto>>();

            if (!_currentUser.IsAdmin)
            {
                validationResult.ErrorType = ServiceErrorType.Forbidden;
                validationResult.Errors.Add("Only admin can access this endpoint.");

                return validationResult;
            }

            if (pagination.PageNumber < 1 ||
               pagination.PageSize < 1 ||
               pagination.PageSize > 100 ||
               ((long)pagination.PageNumber - 1) * pagination.PageSize > int.MaxValue)
            {
                validationResult.ErrorType = ServiceErrorType.Validation;
                validationResult.Errors.Add("Invalid pagination parameters.");

                return validationResult;
            }

            var pagedTasks = await _taskRepository.GetPagedAsync(
                 pagination.PageNumber,
                 pagination.PageSize);

            validationResult.Success = true;
            validationResult.Data = pagedTasks;

            return validationResult;
        }

    }
}
