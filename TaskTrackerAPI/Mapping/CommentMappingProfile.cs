namespace TaskTrackerAPI.Mapping
{
    public class CommentMappingProfile : Profile
    {
        public CommentMappingProfile()
        {
            CreateMap<Comment, CommentResponseDto>();
        }
    }
}
