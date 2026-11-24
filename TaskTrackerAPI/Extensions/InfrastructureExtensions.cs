using System.Threading.RateLimiting;

namespace TaskTrackerAPI.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            // HttpContextAccessor
            services.AddHttpContextAccessor();

            var allowedOrigins = configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? Array.Empty<string>();

            if (allowedOrigins.Length == 0)
            {
                if (environment.IsDevelopment())
                {
                    allowedOrigins = new[]
                    {
                        "http://localhost:4200",
                        "https://localhost:4200"
                    };
                }
            }

            foreach (var origin in allowedOrigins)
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var parsedOrigin) ||
                    (parsedOrigin.Scheme != Uri.UriSchemeHttp &&
                     parsedOrigin.Scheme != Uri.UriSchemeHttps) ||
                    parsedOrigin.AbsolutePath != "/" ||
                    !string.IsNullOrEmpty(parsedOrigin.Query) ||
                    !string.IsNullOrEmpty(parsedOrigin.Fragment))
                {
                    throw new InvalidOperationException(
                        $"Cors:AllowedOrigins contains an invalid origin: {origin}");
                }
            }

            // CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AngularPolicy", policy =>
                {
                    if (allowedOrigins.Length == 0)
                    {
                        // Standalone API deployment: no browser origin is allowed
                        // until a real frontend origin is explicitly configured.
                        policy.SetIsOriginAllowed(_ => false);
                    }
                    else
                    {
                        policy.WithOrigins(allowedOrigins);
                    }

                    policy.AllowAnyHeader().AllowAnyMethod();
                });
            });

            // Rate Limiting
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode =
                    StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsJsonAsync(
                        new ServiceResult
                        {
                            Success = false,
                            ErrorType = ServiceErrorType.TooManyRequests,
                            Errors = new List<string> { "Too many requests. Please try again later." },
                            TraceId = context.HttpContext.TraceIdentifier
                        },
                        cancellationToken);
                };

                options.AddPolicy("AuthPolicy", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }));
            });

            return services;
        }
    }
}
