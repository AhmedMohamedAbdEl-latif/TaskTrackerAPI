namespace TaskTrackerAPI.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        public CommentService(
            ICommentRepository commentRepository,
            IUserRepository userRepository,
            ITaskRepository taskRepository)
        {
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _taskRepository = taskRepository;
        }

        public async Task<ServiceResult<Comment>> CreateAsync(CreateCommentDto commentDto)
        {
           var validationResult = new ServiceResult<Comment>();

            var userExists = await _userRepository.ExistsAsync(commentDto.UserId);

            if (!userExists)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("User not found");
            }

            var taskExists = await _taskRepository.ExistsAsync(commentDto.TaskId);

            if (!taskExists)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Task not found");
            }

            if(validationResult.Errors.Any())
                return validationResult;

            var comment = new Comment
            {
                UserId = commentDto.UserId,
                TaskId = commentDto.TaskId,
                Content = commentDto.Content,
                CreatedAt = DateTime.UtcNow
            };
            _commentRepository.Create(comment);

            validationResult.Success = true;
            validationResult.Data = comment;

            return validationResult;
        }

        public void Delete(int id)
        {
            _commentRepository.Delete(id);
        }

        public Task<Comment> GetByIdAsync(int id)
        {
            return _commentRepository.GetByIdAsync(id);
        }

        public List<Comment> GetByTaskId(int taskId)
        {
            return _commentRepository.GetByTaskId(taskId);
        }

        public Task<List<Comment>> ListAllAsync()
        {
            return _commentRepository.ListAllAsync();
        }

        public async Task<ServiceResult<Comment>> UpdateAsync(int id, UpdateCommentDto commentDto)
        {
           var validationResult = new ServiceResult<Comment>();

            var existingComment = await _commentRepository.GetByIdAsync(id);

            if(existingComment == null)
            {
                validationResult.ErrorType = ServiceErrorType.NotFound;
                validationResult.Errors.Add("Comment not found");
                return validationResult;
            }

            existingComment.Content = commentDto.Content;

            _commentRepository.Update(existingComment);

            validationResult.Success = true;
            validationResult.Data = existingComment;

            return validationResult;
        }
    }
}
