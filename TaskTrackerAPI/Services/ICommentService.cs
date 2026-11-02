namespace TaskTrackerAPI.Services
{
    public interface ICommentService
    {
        Task<List<CommentResponseDto>> ListAllAsync();
        Task<ServiceResult<CommentResponseDto>> GetByIdAsync(int id);
        Task<ServiceResult<List<CommentResponseDto>>> GetByTaskIdAsync(int taskId);
        Task<ServiceResult<CommentResponseDto>> CreateAsync(CreateCommentDto commentDto);
        Task<ServiceResult<CommentResponseDto>> UpdateAsync(int id, UpdateCommentDto commentDto);
        Task<ServiceResult> DeleteAsync(int id);

        Task<List<CommentResponseDto>> GetAllProjectedAsync();
    }
}
