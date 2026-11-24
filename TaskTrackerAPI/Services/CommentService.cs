namespace TaskTrackerAPI.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public CommentService(
            ICommentRepository commentRepository,
            IUserRepository userRepository,
            ITaskRepository taskRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser
)
        {
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _taskRepository = taskRepository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<ServiceResult<CommentResponseDto>> CreateAsync(CreateCommentDto commentDto, CancellationToken cancellationToken = default)
        {
           var validationResult = new ServiceResult<CommentResponseDto>();

            var userId = _currentUser.UserId;

            if (!userId.HasValue)
            {
                validationResult.ErrorType = ServiceErrorType.Unauthorized;
                validationResult.Errors.Add("User is not authenticated.");
                return validationResult;
            }

            var userExists = await _userRepository.ExistsAsync(userId.Value, cancellationToken);

            if (!userExists)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("User not found.");
                return validationResult;
            }

            var taskExists = await _taskRepository.ExistsAsync(commentDto.TaskId, cancellationToken);

            if (!taskExists)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Task not found");
                return validationResult;
            }

            if (!_currentUser.IsAdmin)
            {
                var canAccess = await _taskRepository.CanUserAccessTaskAsync(
                    commentDto.TaskId,
                    userId.Value,
                    cancellationToken);

                if (!canAccess)
                {
                    validationResult.ErrorType = ServiceErrorType.Forbidden;
                    validationResult.Errors.Add(
                        "You are not allowed to add comments to this task.");

                    return validationResult;
                }
            }

            var comment = new Comment
            {
                UserId = userId.Value,
                TaskId = commentDto.TaskId,
                Content = commentDto.Content.Trim(),
                CreatedAt = DateTime.UtcNow
            };


            _commentRepository.Create(comment);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            validationResult.Success = true;

            validationResult.Data = CommentMapper.ToResponseDto(comment);
            return validationResult;
        }
        public async Task<ServiceResult<CommentResponseDto>> UpdateAsync(int id, UpdateCommentDto commentDto, CancellationToken cancellationToken = default)
        {
           var validationResult = new ServiceResult<CommentResponseDto>();

            var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);

            if(comment == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Comment not found");
                return validationResult;
            }
            var currentUserId = _currentUser.UserId;

            if (!_currentUser.IsAdmin)
            {
                if (!currentUserId.HasValue)
                {
                    validationResult.ErrorType = ServiceErrorType.Unauthorized;
                    validationResult.Errors.Add("User is not authenticated.");
                    return validationResult;
                }

                if (comment.UserId != currentUserId.Value)
                {
                    validationResult.ErrorType = ServiceErrorType.Forbidden;
                    validationResult.Errors.Add("You are not allowed to update this comment.");
                    return validationResult;
                }
            }

            comment.Content = commentDto.Content.Trim();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            validationResult.Success = true;

            validationResult.Data = CommentMapper.ToResponseDto(comment);

            return validationResult;
        }

        public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var validationResult = new ServiceResult();

            var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);

            if (comment == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Comment not found.");
                return validationResult;
            }

            var currentUserId = _currentUser.UserId;

            if (!_currentUser.IsAdmin)
            {
                if(!currentUserId.HasValue)
                {
                    validationResult.ErrorType = ServiceErrorType.Unauthorized;
                    validationResult.Errors.Add("User is not authenticated.");
                    return validationResult;
                }

                if(comment.UserId != currentUserId.Value)
                {
                    validationResult.ErrorType = ServiceErrorType.Forbidden;
                    validationResult.Errors.Add("You are not allowed to delete this comment.");
                    return validationResult;
                }
            }

            _commentRepository.Delete(comment);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            validationResult.Success = true;
            return validationResult;
        }

        public async Task<ServiceResult<CommentResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var validationResult = new ServiceResult<CommentResponseDto>();

            var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);

            if (comment == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Comment not found.");
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
                    comment.TaskId,
                    _currentUser.UserId.Value,
                    cancellationToken);

                if (!canAccess)
                {
                    validationResult.ErrorType = ServiceErrorType.Forbidden;
                    validationResult.Errors.Add("You are not allowed to view this comment.");
                    return validationResult;
                }
            }

            validationResult.Success = true;
            validationResult.Data = CommentMapper.ToResponseDto(comment);

            return validationResult;
        }

        public async Task<ServiceResult<PagedResultDto<CommentResponseDto>>> GetByTaskIdAsync(
            int taskId,
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default)
        {
            var validationResult = new ServiceResult<PagedResultDto<CommentResponseDto>>();

            var taskExists = await _taskRepository.ExistsAsync(taskId, cancellationToken);

            if (!taskExists)
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
                    taskId,
                    _currentUser.UserId.Value,
                    cancellationToken);

                if (!canAccess)
                {
                    validationResult.ErrorType = ServiceErrorType.Forbidden;
                    validationResult.Errors.Add("You are not allowed to view comments for this task.");
                    return validationResult;
                }
            }

            validationResult.Success = true;
            validationResult.Data = await _commentRepository.GetPagedAsync(
                pagination,
                taskId,
                cancellationToken);

            return validationResult;
        }

        public async Task<PagedResultDto<CommentResponseDto>> ListAllAsync(
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default)
        {
            return await _commentRepository.GetPagedAsync(pagination, cancellationToken: cancellationToken);
        }


        public async Task<PagedResultDto<CommentResponseDto>> GetAllProjectedAsync(
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default)
        {
            return await _commentRepository.GetPagedAsync(pagination, cancellationToken: cancellationToken);
        }

    }
}
