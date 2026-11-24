using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

using TaskTrackerAPI.Extensions;
using System.Linq;


namespace TaskTrackerAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;
                options.KnownProxies.Clear();
                options.KnownIPNetworks.Clear();

                var trustedProxies = builder.Configuration
                    .GetSection("ReverseProxy:KnownProxies")
                    .Get<string[]>() ?? Array.Empty<string>();

                foreach (var proxyAddress in trustedProxies)
                {
                    if (!IPAddress.TryParse(proxyAddress, out var address))
                    {
                        throw new InvalidOperationException(
                            $"ReverseProxy:KnownProxies contains an invalid IP address: {proxyAddress}");
                    }

                    options.KnownProxies.Add(address);
                }

                options.ForwardLimit = Math.Max(trustedProxies.Length, 1);
            });

            // MVC / API
            builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                    .Values
                    .SelectMany(modelState => modelState.Errors)
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "Invalid input."
                    : error.ErrorMessage)
                    .ToList();

                    var validationResult = new ServiceResult
                    {
                        Success = false,
                        ErrorType = ServiceErrorType.Validation,
                        Errors = errors,
                        TraceId = context.HttpContext.TraceIdentifier
                    };

                    return new BadRequestObjectResult(validationResult);
                };
            });

            // Global exception handling and API errors use ServiceResult.
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            // Required by UseExceptionHandler() as its fallback response writer.
            // The registered GlobalExceptionHandler handles normal API exceptions first.
            builder.Services.AddProblemDetails();

            // Application Services: including DbContext, Repositories, Services, Authentication, and Current User Services
            builder.Services.AddApplicationServices(
                 builder.Configuration,
                 builder.Environment);


            // Authentication and Authorization
            builder.Services.AddJwtAuthentication(builder.Configuration);

            // Rate Limiting, CORS, and HttpContextAccessor
            builder.Services.AddInfrastructureServices(
                builder.Configuration,
                builder.Environment);

            // Swagger
            builder.Services.AddSwaggerDocumentation();

            var app = builder.Build();

            await using (var scope = app.Services.CreateAsyncScope())
            {
                await scope.ServiceProvider.GetRequiredService<Services.Auth.AdminBootstrapper>()
                    .RunAsync();
            }

            // Configure the HTTP request pipeline: including Swagger,
            // exception handling, CORS, Rate Limiting, Authentication, and Authorization.
            app.ConfigurePipeline();

            app.Run();
        }
    }
}
