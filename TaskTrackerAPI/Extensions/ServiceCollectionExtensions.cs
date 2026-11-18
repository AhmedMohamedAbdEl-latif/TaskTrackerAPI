using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskTrackerAPI.Repository.Auth;
using TaskTrackerAPI.Services.Auth;

namespace TaskTrackerAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            // Database
            services.AddDbContextFactory<TaskTrackerContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("TaskTrackerDatabase"));

                if (environment.IsDevelopment())
                {
                    options.LogTo(Console.WriteLine, LogLevel.Information);
                }
                else
                {
                    options.LogTo(Console.WriteLine, LogLevel.Warning);
                }
            });

            // Repository Layer
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ILoginAttemptRepository, LoginAttemptRepository>();

            // Service Layer
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IUserService, UserService>();

            // Authentication Services
            services.AddScoped<PasswordHasher<User>>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ILoginAttemptService, LoginAttemptService>();

            // Unit of Work
            services.AddScoped<IUnitOfWork,
                TaskTrackerAPI.UnitOfWork.UnitOfWork>();

            // Unit of Work Factory
            services.AddSingleton<ILoginAttemptUnitOfWorkFactory, LoginAttemptUnitOfWorkFactory>();


            // Current User
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ICurrentUserValidationService,
                CurrentUserValidationService>();

            return services;
        }
    }
}