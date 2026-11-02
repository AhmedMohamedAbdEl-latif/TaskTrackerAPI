namespace TaskTrackerAPI.Mapping
{
    public class UserMappingProfile: Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, UserResponseDto>();
        }
    }
}
