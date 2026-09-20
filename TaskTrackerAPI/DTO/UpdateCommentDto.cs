using System.ComponentModel.DataAnnotations;

namespace TaskTrackerAPI.DTO
{
    public class UpdateCommentDto
    {
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }
    }
}
