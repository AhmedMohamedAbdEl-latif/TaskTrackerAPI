using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskTrackerAPI.Models
{
    public class ProjectTask
    {
        public int Id { get; set; }
        [MaxLength(200)]
        [Required]
        public string Title { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public TaskStatus Status { get; set; }
        [ForeignKey("Project")]
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        [ForeignKey("AssignedUser")]
        public int AssignedUserId { get; set; }
        public User AssignedUser { get; set; } = null!;
    }

    public enum TaskStatus
    {
        ToDo,
        InProgress,
        Done
    }
}
