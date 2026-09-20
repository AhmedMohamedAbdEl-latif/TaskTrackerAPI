namespace TaskTrackerAPI.UnitOfWork
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly TaskTrackerContext context;

        public UnitOfWork(TaskTrackerContext _context)
        {
            context = _context;
        }
        public async Task<int> SaveChangesAsync()
        {
            return await context.SaveChangesAsync();
        }
    }
}
