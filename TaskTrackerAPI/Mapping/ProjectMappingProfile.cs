using AutoMapper;

namespace TaskTrackerAPI.Mapping
{
    public class ProjectMappingProfile: Profile
    {
        public ProjectMappingProfile()
        {
            CreateMap<Project, ProjectResponseDto>();
        }
    }
}
