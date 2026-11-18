namespace TaskTrackerAPI.Repository
{
    public interface ICommentRepository
    {
        Task<List<Comment>> ListAllAsync();
        Task<Comment?> GetByIdAsync(int id);
        Task<List<Comment>> GetByTaskIdAsync(int taskId);

        Task<List<CommentResponseDto>> GetAllProjectedAsync();
        Task<bool> HasCommentsByTaskIdAsync(int taskId);
        void Create(Comment comment);
        void Update(Comment comment);
        void Delete(Comment comment);
    }
}
