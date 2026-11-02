namespace TaskTrackerAPI.DTO
{
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public ProjectTaskStatus Status { get; set; }
        public int ProjectId { get; set; }
        public int AssignedUserId { get; set; }
    }
}
