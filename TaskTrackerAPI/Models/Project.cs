using System.ComponentModel.DataAnnotations;

namespace TaskTrackerAPI.Models
{
    public class Project
    {
        public int Id { get; set; }
        [MaxLength(100)]
        [Required]
        public string Name { get; set; } = string.Empty;
        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;
    }
}
