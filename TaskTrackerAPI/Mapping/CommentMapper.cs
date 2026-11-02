namespace TaskTrackerAPI.Mapping
{
    public class CommentMapper
    {
        public static CommentResponseDto ToResponseDto(Comment comment)
        {
            return new CommentResponseDto
            {
                Id = comment.Id,
                UserId = comment.UserId,
                TaskId = comment.TaskId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };
        }
    }
}
