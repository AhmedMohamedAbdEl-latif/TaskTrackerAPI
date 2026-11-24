using TaskTrackerAPI.DTO.Pagination;

namespace TaskTrackerAPI.Repository
{
    public interface ICommentRepository
    {
        Task<List<Comment>> ListAllAsync(CancellationToken cancellationToken = default);
        Task<Comment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Comment>> GetByTaskIdAsync(int taskId, CancellationToken cancellationToken = default);
        Task<List<CommentResponseDto>> GetAllProjectedAsync(CancellationToken cancellationToken = default);
        Task<PagedResultDto<CommentResponseDto>> GetPagedAsync(
            PaginationQueryDto pagination,
            int? taskId = null,
            CancellationToken cancellationToken = default);
        Task<bool> HasCommentsByTaskIdAsync(int taskId, CancellationToken cancellationToken = default);
        void Create(Comment comment);
        void Update(Comment comment);
        void Delete(Comment comment);
    }
}
