using System.ComponentModel.DataAnnotations;

namespace TaskTrackerAPI.DTO
{
    public class ProjectDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [Required]
        public int? OwnerId { get; set; }
    }
}
