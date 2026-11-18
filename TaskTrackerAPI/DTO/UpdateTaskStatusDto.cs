using System.ComponentModel.DataAnnotations;

namespace TaskTrackerAPI.DTO
{
    public class UpdateTaskStatusDto
    {
        [Required(ErrorMessage = "Status is required.")]
        [EnumDataType(typeof(ProjectTaskStatus), ErrorMessage = "Invalid task status.")]
        public ProjectTaskStatus? Status { get; set; }

    }
}
