namespace TaskTrackerAPI.Services.Auth
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
