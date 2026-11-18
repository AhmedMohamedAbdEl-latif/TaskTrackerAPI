using System.ComponentModel.DataAnnotations;
using TaskTrackerAPI.Validation;

namespace TaskTrackerAPI.DTO
{
    public class UpdateCommentDto
    {
        [Required]
        [NotWhitespace(ErrorMessage = "Comment content cannot be empty or whitespace.")]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;
    }
}
