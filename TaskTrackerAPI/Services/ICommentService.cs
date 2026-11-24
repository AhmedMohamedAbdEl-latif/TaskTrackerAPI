namespace TaskTrackerAPI.Services
{
    public interface ICommentService
    {
        Task<PagedResultDto<CommentResponseDto>> ListAllAsync(
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default);
        Task<ServiceResult<CommentResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ServiceResult<PagedResultDto<CommentResponseDto>>> GetByTaskIdAsync(
            int taskId,
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default);
        Task<ServiceResult<CommentResponseDto>> CreateAsync(CreateCommentDto commentDto, CancellationToken cancellationToken = default);
        Task<ServiceResult<CommentResponseDto>> UpdateAsync(int id, UpdateCommentDto commentDto, CancellationToken cancellationToken = default);
        Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);

        Task<PagedResultDto<CommentResponseDto>> GetAllProjectedAsync(
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default);
    }
}
