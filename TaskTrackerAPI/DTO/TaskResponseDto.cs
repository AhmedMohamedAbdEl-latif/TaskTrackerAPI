namespace TaskTrackerAPI.DTO
{
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public ProjectTaskStatus Status { get; set; }
        public int ProjectId { get; set; }
        public int AssignedUserId { get; set; }
    }
}
