using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskTrackerAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
        [ForeignKey("Task")]
        public int TaskId { get; set; }
        public ProjectTask Task { get; set; }
        [MaxLength(1000)]

        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
