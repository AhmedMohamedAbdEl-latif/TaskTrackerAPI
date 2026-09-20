namespace TaskTrackerAPI.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
    }
}
