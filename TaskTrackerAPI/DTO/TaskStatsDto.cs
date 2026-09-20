namespace TaskTrackerAPI.DTO
{
    public class TaskStatsDto
    {
        public int ToDo { get; set; }
        public int InProgress { get; set; }
        public int Done { get; set; }
        public int OverDue { get; set; }
    }
}
