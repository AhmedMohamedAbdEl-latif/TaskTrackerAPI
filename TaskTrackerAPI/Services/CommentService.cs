namespace TaskTrackerAPI.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public CommentService(
            ICommentRepository commentRepository,
            IUserRepository userRepository,
            ITaskRepository taskRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _taskRepository = taskRepository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<ServiceResult<CommentResponseDto>> CreateAsync(CreateCommentDto commentDto)
        {
           var validationResult = new ServiceResult<CommentResponseDto>();

            var userId = _currentUser.UserId;

            if (!userId.HasValue)
            {
                validationResult.ErrorType = ServiceErrorType.Unauthorized;
                validationResult.Errors.Add("User is not authenticated.");
                return validationResult;
            }

            var userExists = await _userRepository.ExistsAsync(userId.Value);

            if (!userExists)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("User not found.");
                return validationResult;
            }

            var taskExists = await _taskRepository.ExistsAsync(commentDto.TaskId);

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
                    userId.Value);

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

            await _unitOfWork.SaveChangesAsync();

            validationResult.Success = true;
            //validationResult.Data = CommentMapper.ToResponseDto(comment);

            validationResult.Data = _mapper.Map<CommentResponseDto>(comment);
            return validationResult;
        }
        public async Task<ServiceResult<CommentResponseDto>> UpdateAsync(int id, UpdateCommentDto commentDto)
        {
           var validationResult = new ServiceResult<CommentResponseDto>();

            var comment = await _commentRepository.GetByIdAsync(id);

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

            await _unitOfWork.SaveChangesAsync();

            validationResult.Success = true;
            //validationResult.Data = CommentMapper.ToResponseDto(comment);

            validationResult.Data = _mapper.Map<CommentResponseDto>(comment);

            return validationResult;
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var validationResult = new ServiceResult();

            var comment = await _commentRepository.GetByIdAsync(id);

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

            await _unitOfWork.SaveChangesAsync();

            validationResult.Success = true;
            return validationResult;
        }

        public async Task<ServiceResult<CommentResponseDto>> GetByIdAsync(int id)
        {
            var validationResult = new ServiceResult<CommentResponseDto>();

            var comment = await _commentRepository.GetByIdAsync(id);

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
                    _currentUser.UserId.Value);

                if (!canAccess)
                {
                    validationResult.ErrorType = ServiceErrorType.Forbidden;
                    validationResult.Errors.Add("You are not allowed to view this comment.");
                    return validationResult;
                }
            }

            validationResult.Success = true;
            validationResult.Data = _mapper.Map<CommentResponseDto>(comment);

            return validationResult;
        }

        public async Task<ServiceResult<List<CommentResponseDto>>> GetByTaskIdAsync(int taskId)
        {
            var validationResult = new ServiceResult<List<CommentResponseDto>>();

            var taskExists = await _taskRepository.ExistsAsync(taskId);

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
                    _currentUser.UserId.Value);

                if (!canAccess)
                {
                    validationResult.ErrorType = ServiceErrorType.Forbidden;
                    validationResult.Errors.Add("You are not allowed to view comments for this task.");
                    return validationResult;
                }
            }

            var comments = await _commentRepository.GetByTaskIdAsync(taskId);

            validationResult.Success = true;
            validationResult.Data = _mapper.Map<List<CommentResponseDto>>(comments);

            return validationResult;
        }

        public async Task<List<CommentResponseDto>> ListAllAsync()
        {
            var comments = await _commentRepository.ListAllAsync();

            //return comments
            //    .Select(CommentMapper.ToResponseDto)
            //    .ToList();

            return _mapper.Map<List<CommentResponseDto>>(comments);
        }


        public async Task<List<CommentResponseDto>> GetAllProjectedAsync()
        {
            return await _commentRepository.GetAllProjectedAsync();
        }

    }
}
