using AutoMapper;

namespace TaskTrackerAPI.Mapping
{
    public class TaskMappingProfile : Profile
    {
        public TaskMappingProfile()
        {
            CreateMap<ProjectTask, TaskResponseDto>();
        }

    }
}
