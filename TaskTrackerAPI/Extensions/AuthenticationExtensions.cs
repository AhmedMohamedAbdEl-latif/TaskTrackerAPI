using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TaskTrackerAPI.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSecret = configuration["JWT:SecretKey"];
            var jwtIssuer = configuration["JWT:IssuerIP"];
            var jwtAudience = configuration["JWT:AudienceIP"];
            var durationValue = configuration["JWT:DurationInMinutes"];

            if (string.IsNullOrWhiteSpace(jwtSecret) ||
                Encoding.UTF8.GetByteCount(jwtSecret) < 32 ||
                jwtSecret.Contains("YOUR_SUPER_SECRET_KEY_HERE",
                    StringComparison.OrdinalIgnoreCase) ||
                jwtSecret.Contains("CHANGE_ME",
                    StringComparison.OrdinalIgnoreCase) ||
                jwtSecret.Contains("PLACEHOLDER",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "JWT SecretKey must be a valid secret containing at least 32 bytes.");
            }

            if (string.IsNullOrWhiteSpace(jwtIssuer))
            {
                throw new InvalidOperationException(
                    "JWT IssuerIP must be configured.");
            }

            if (string.IsNullOrWhiteSpace(jwtAudience))
            {
                throw new InvalidOperationException(
                    "JWT AudienceIP must be configured.");
            }

            if (!int.TryParse(durationValue, out var duration) ||
                duration <= 0 ||
                duration > 30)
            {
                throw new InvalidOperationException(
                    "JWT DurationInMinutes must be between 1 and 30.");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,

                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSecret)),

                        ClockSkew = TimeSpan.FromSeconds(30)
                    };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new ServiceResult
                        {
                            Success = false,
                            ErrorType = ServiceErrorType.Unauthorized,
                            Errors = new List<string> { "Authentication is required or the token is invalid." },
                            TraceId = context.HttpContext.TraceIdentifier
                        }, context.HttpContext.RequestAborted);
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(new ServiceResult
                        {
                            Success = false,
                            ErrorType = ServiceErrorType.Forbidden,
                            Errors = new List<string> { "You are not allowed to perform this action." },
                            TraceId = context.HttpContext.TraceIdentifier
                        }, context.HttpContext.RequestAborted);
                    },
                    OnTokenValidated = async context =>
                    {
                        var validationService = context.HttpContext
                            .RequestServices
                            .GetRequiredService<
                                ICurrentUserValidationService>();

                        var isValid = await validationService
                            .ValidateAsync(
                                context.Principal!,
                                context.HttpContext.RequestAborted);

                        if (!isValid)
                        {
                            context.Fail(
                                "The authenticated user does not exist or is invalid.");
                        }
                    }
                };
            });

            services.AddAuthorization();

            return services;
        }
    }
}
