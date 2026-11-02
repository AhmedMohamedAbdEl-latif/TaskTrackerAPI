using System.ComponentModel.DataAnnotations;

namespace TaskTrackerAPI.DTO
{
    public class ProjectDto
    {
        [Required(ErrorMessage = "Project name is required.")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Owner ID is required.")]
        public int? OwnerId { get; set; }
    }
}
