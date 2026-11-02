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

            if (validationResult.Errors.Any())
                return validationResult;

            var comment = new Comment
            {
                UserId = userId.Value,
                TaskId = commentDto.TaskId,
                Content = commentDto.Content,
                CreatedAt = DateTime.UtcNow
            };


            _commentRepository.Create(comment);

            await _unitOfWork.SaveChangesAsync();

            validationResult.Success = true;
            //validationResult.Data = CommentMapper.ToResponseDto(comment);

            validationResult.Data = _mapper.Map<CommentResponseDto>(comment);
            return validationResult;
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var result = new ServiceResult();

            var comment = await _commentRepository.GetByIdAsync(id);

            if (comment == null)
            {
                result.ErrorType = ServiceErrorType.NotFound;
                result.Errors.Add("Comment not found.");
                return result;
            }

            if (!_currentUser.IsAdmin && !_currentUser.UserId.HasValue)
            {
                result.ErrorType = ServiceErrorType.Unauthorized;
                result.Errors.Add("User is not authenticated.");
                return result;
            }

            if (!_currentUser.IsAdmin &&
                comment.UserId != _currentUser.UserId.Value)
            {
                result.ErrorType = ServiceErrorType.Forbidden;
                result.Errors.Add("You are not allowed to delete this comment.");
                return result;
            }

            _commentRepository.Delete(id);

            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            return result;
        }

        public async Task<ServiceResult<CommentResponseDto>> GetByIdAsync(int id)
        {
            var result = new ServiceResult<CommentResponseDto>();

            var comment = await _commentRepository.GetByIdAsync(id);

            if (comment == null)
            {
                result.ErrorType = ServiceErrorType.NotFound;
                result.Errors.Add("Comment not found.");
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
                    comment.TaskId,
                    _currentUser.UserId.Value);

                if (!canAccess)
                {
                    result.ErrorType = ServiceErrorType.Forbidden;
                    result.Errors.Add("You are not allowed to view this comment.");
                    return result;
                }
            }

            result.Success = true;
            result.Data = _mapper.Map<CommentResponseDto>(comment);

            return result;
        }

        public async Task<ServiceResult<List<CommentResponseDto>>> GetByTaskIdAsync(int taskId)
        {
            var result = new ServiceResult<List<CommentResponseDto>>();

            var taskExists = await _taskRepository.ExistsAsync(taskId);

            if (!taskExists)
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
                    taskId,
                    _currentUser.UserId.Value);

                if (!canAccess)
                {
                    result.ErrorType = ServiceErrorType.Forbidden;
                    result.Errors.Add("You are not allowed to view comments for this task.");
                    return result;
                }
            }

            var comments = _commentRepository.GetByTaskId(taskId);

            result.Success = true;
            result.Data = _mapper.Map<List<CommentResponseDto>>(comments);

            return result;
        }

        public async Task<List<CommentResponseDto>> ListAllAsync()
        {
            var comments = await _commentRepository.ListAllAsync();

            //return comments
            //    .Select(CommentMapper.ToResponseDto)
            //    .ToList();

            return _mapper.Map<List<CommentResponseDto>>(comments);
        }

        public async Task<ServiceResult<CommentResponseDto>> UpdateAsync(int id, UpdateCommentDto commentDto)
        {
           var validationResult = new ServiceResult<CommentResponseDto>();

            var existingComment = await _commentRepository.GetByIdAsync(id);

            if(existingComment == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Comment not found");
                return validationResult;
            }

            if (!_currentUser.IsAdmin && !_currentUser.UserId.HasValue)
            {
                validationResult.ErrorType = ServiceErrorType.Unauthorized;
                validationResult.Errors.Add("User is not authenticated.");
                return validationResult;
            }

            if (!_currentUser.IsAdmin &&
                existingComment.UserId != _currentUser.UserId.Value)
            {
                validationResult.ErrorType = ServiceErrorType.Forbidden;
                validationResult.Errors.Add("You are not allowed to update this comment.");
                return validationResult;
            }

            existingComment.Content = commentDto.Content;

            // _commentRepository.Update(existingComment);

            await _unitOfWork.SaveChangesAsync();

            validationResult.Success = true;
            //validationResult.Data = CommentMapper.ToResponseDto(existingComment);

            validationResult.Data = _mapper.Map<CommentResponseDto>(existingComment);

            return validationResult;
        }

        public async Task<List<CommentResponseDto>> GetAllProjectedAsync()
        {
            return await _commentRepository.GetAllProjectedAsync();
        }

    }
}
