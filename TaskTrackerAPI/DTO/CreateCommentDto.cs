using System.ComponentModel.DataAnnotations;
using TaskTrackerAPI.Validation;

namespace TaskTrackerAPI.DTO
{
    public class CreateCommentDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Task ID must be greater than 0.")]
        public int TaskId { get; set; }

        [NotWhitespace(ErrorMessage = "Comment content cannot be empty or whitespace.")]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;
    }
}
