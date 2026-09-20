using System.ComponentModel.DataAnnotations;

namespace TaskTrackerAPI.DTO
{
    public class CreateCommentDto
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int TaskId { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }
    }
}
