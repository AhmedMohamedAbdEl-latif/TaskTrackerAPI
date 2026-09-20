namespace TaskTrackerAPI.Repository
{
    public interface ICommentRepository
    {
        Task<List<Comment>> ListAllAsync();
        Task<Comment?> GetByIdAsync(int id);

        List<Comment?> GetByTaskId(int taskId);
        void Create(Comment comment);
        void Update(Comment comment);
        void Delete(int id);
    }
}
