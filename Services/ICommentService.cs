namespace TaskTrackerAPI.Services
{
    public interface ICommentService
    {
        Task<List<Comment>> ListAllAsync();
        Task<Comment?> GetByIdAsync(int id);
        List<Comment?> GetByTaskId(int taskId);
        Task<ServiceResult<Comment>> CreateAsync(CreateCommentDto commentDto);
        Task<ServiceResult<Comment>> UpdateAsync(int id, UpdateCommentDto commentDto);
        void Delete(int id);
    }
}
