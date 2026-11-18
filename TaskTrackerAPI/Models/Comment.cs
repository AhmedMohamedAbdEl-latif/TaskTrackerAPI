using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskTrackerAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        [ForeignKey("Task")]
        public int TaskId { get; set; }
        public ProjectTask Task { get; set; } = null!;
        [MaxLength(1000)]

        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
