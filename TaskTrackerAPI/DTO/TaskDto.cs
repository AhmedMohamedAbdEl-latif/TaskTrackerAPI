using System.ComponentModel.DataAnnotations;
using TaskTrackerAPI.Validation;

namespace TaskTrackerAPI.DTO
{
    public class TaskDto
    {
        [Required(ErrorMessage = " Task title is required.")]
        [NotWhitespace(ErrorMessage = "Task title cannot be empty or whitespace.")]

        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Task description is required.")]
        [NotWhitespace(ErrorMessage = "Task description cannot be empty or whitespace.")]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [System.Text.Json.Serialization.JsonConverter(typeof(NullableUtcDateTimeJsonConverter))]
        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [EnumDataType(typeof(ProjectTaskStatus), ErrorMessage = "Invalid task status.")]
        public ProjectTaskStatus? Status { get; set; }

        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }

        [Range(1, int.MaxValue)]
        public int AssignedUserId { get; set; }
    }

}
