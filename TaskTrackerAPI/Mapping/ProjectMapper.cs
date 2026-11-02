namespace TaskTrackerAPI.Mapping
{
    public class ProjectMapper
    {
        public static ProjectResponseDto ToResponseDto(Project project)
        {
            return new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                OwnerId = project.OwnerId
            };
        }
    }
}
