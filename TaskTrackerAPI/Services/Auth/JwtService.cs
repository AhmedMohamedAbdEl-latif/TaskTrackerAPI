using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TaskTrackerAPI.Services.Auth
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GenerateToken(User user)
        {
            var key = _configuration["JWT:SecretKey"];
            var issuerIP = _configuration["JWT:IssuerIP"];
            var audienceIP = _configuration["JWT:AudienceIP"];

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException("JWT:SecretKey is missing.");
            }

            var durationValue = _configuration["JWT:DurationInMinutes"];

            if (!int.TryParse(durationValue, out var duration) || duration <= 0)
            {
                throw new InvalidOperationException("Jwt:DurationInMinutes must be a valid positive integer.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuerIP,
                audience: audienceIP,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(duration),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
