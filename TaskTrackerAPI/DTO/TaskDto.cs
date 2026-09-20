using System.ComponentModel.DataAnnotations;

namespace TaskTrackerAPI.DTO
{
    public class TaskDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        [Required]
        public DateTime DueDate { get; set; }
        [Required]
        public ProjectTaskStatus Status { get; set; }
        [Required]
        public int ProjectId { get; set; }
        [Required]
        public int AssignedUserId { get; set; }
    }

}
