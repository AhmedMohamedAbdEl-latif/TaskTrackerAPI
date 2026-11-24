namespace TaskTrackerAPI.DTO.Auth
{
    public sealed class TwoFactorSetupDto
    {
        public string SharedKey { get; init; } = string.Empty;
        public string AuthenticatorUri { get; init; } = string.Empty;
    }
}
