using Microsoft.OpenApi;

namespace TaskTrackerAPI.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(
            this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Task Tracker API",
                    Description = "Task and project management API. Success and error bodies use a consistent envelope; collection endpoints support pagination."
                });

                swagger.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description =
                            "Enter 'Bearer' [space] and then your valid token"
                    });

                swagger.OperationFilter<AuthorizeOperationFilter>();
            });

            return services;
        }
    }
}
