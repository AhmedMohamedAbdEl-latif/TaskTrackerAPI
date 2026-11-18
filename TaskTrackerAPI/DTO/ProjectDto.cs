using System.ComponentModel.DataAnnotations;
using TaskTrackerAPI.Validation;

namespace TaskTrackerAPI.DTO
{
    public class ProjectDto
    {
        [Required(ErrorMessage = "Project name is required.")]
        [NotWhitespace(ErrorMessage = "Project name cannot be empty or whitespace.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Owner ID is required.")]
        [Range(1, int.MaxValue)]
        public int OwnerId { get; set; }
    }
}
