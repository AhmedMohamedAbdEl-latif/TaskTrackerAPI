using Microsoft.AspNetCore.Mvc;

using TaskTrackerAPI.Extensions;
using System.Linq;


namespace TaskTrackerAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
                        Errors = errors
                    };

                    return new BadRequestObjectResult(validationResult);
                };
            });

            // Global Exception Handling, ProblemDetails, and Logging
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            // Application Services: including DbContext, Repositories, Services, Authentication, and Current User Services
            builder.Services.AddApplicationServices(
                 builder.Configuration,
                 builder.Environment);


            // Authentication and Authorization
            builder.Services.AddJwtAuthentication(builder.Configuration);

            // Rate Limiting, CORS, AutoMapper, HttpContextAccessor
            builder.Services.AddInfrastructureServices();

            // Swagger
            builder.Services.AddSwaggerDocumentation();

            var app = builder.Build();

            // Configure the HTTP request pipeline: including Swagger,
            // Exception Handling, ProblemDetails, CORS, Rate Limiting, Authentication, and Authorization
            app.ConfigurePipeline();

            app.Run();
        }
    }
}
