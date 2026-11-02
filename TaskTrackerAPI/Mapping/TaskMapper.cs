namespace TaskTrackerAPI.Mapping
{
    public class TaskMapper
    {
        public static TaskResponseDto ToResponseDto(ProjectTask task)
        {
            ArgumentNullException.ThrowIfNull(task, nameof(task));

            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Status = task.Status,
                ProjectId = task.ProjectId,
                AssignedUserId = task.AssignedUserId
            };
        }
    }
}
