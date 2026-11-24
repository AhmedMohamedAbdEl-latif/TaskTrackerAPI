using System.Text.Json.Serialization;

namespace TaskTrackerAPI.DTO
{
    public class CommentResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TaskId { get; set; }
        public string Content { get; set; } = string.Empty;

        [JsonConverter(typeof(TaskTrackerAPI.Validation.UtcDateTimeJsonConverter))]
        public DateTime CreatedAt { get; set; }
    }
}
