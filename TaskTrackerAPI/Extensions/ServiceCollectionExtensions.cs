using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskTrackerAPI.Repository.Auth;
using TaskTrackerAPI.Services.Auth;
using System.Security.Cryptography.X509Certificates;

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
                    options.LogTo(Console.WriteLine, LogLevel.Information);
                else
                    options.LogTo(Console.WriteLine, LogLevel.Warning);
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

            // Data Protection key ring must persist across restarts and instances.
            var dataProtection = services.AddDataProtection()
                .SetApplicationName("TaskTrackerAPI");
            var configuredKeysPath = configuration["DataProtection:KeysPath"];

            if (environment.IsDevelopment())
            {
                var developmentKeysPath = string.IsNullOrWhiteSpace(configuredKeysPath)
                    ? Path.Combine(environment.ContentRootPath, ".keys")
                    : Path.IsPathRooted(configuredKeysPath)
                        ? configuredKeysPath
                        : Path.Combine(environment.ContentRootPath, configuredKeysPath);
                Directory.CreateDirectory(developmentKeysPath);
                dataProtection.PersistKeysToFileSystem(new DirectoryInfo(developmentKeysPath));
            }
            else
            {
                var certificatePath = configuration["DataProtection:CertificatePath"];
                var certificatePassword = configuration["DataProtection:CertificatePassword"];
                if (string.IsNullOrWhiteSpace(configuredKeysPath) ||
                    string.IsNullOrWhiteSpace(certificatePath) ||
                    string.IsNullOrWhiteSpace(certificatePassword))
                {
                    throw new InvalidOperationException(
                        "Production requires DataProtection:KeysPath, CertificatePath, and CertificatePassword so two-factor secrets remain decryptable and the key ring is protected at rest.");
                }

                var productionKeysPath = Path.IsPathRooted(configuredKeysPath)
                    ? configuredKeysPath
                    : Path.Combine(environment.ContentRootPath, configuredKeysPath);
                var resolvedCertificatePath = Path.IsPathRooted(certificatePath)
                    ? certificatePath
                    : Path.Combine(environment.ContentRootPath, certificatePath);
                Directory.CreateDirectory(productionKeysPath);
                var certificate = X509CertificateLoader.LoadPkcs12FromFile(
                    resolvedCertificatePath,
                    certificatePassword);
                dataProtection
                    .PersistKeysToFileSystem(new DirectoryInfo(productionKeysPath))
                    .ProtectKeysWithCertificate(certificate);
            }

            // Authentication Services
            services.AddScoped<PasswordHasher<User>>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<AdminBootstrapper>();
            services.AddHostedService<LoginAttemptRetentionService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ILoginAttemptService, LoginAttemptService>();

            // Unit of Work
            services.AddScoped<IUnitOfWork,
                TaskTrackerAPI.UnitOfWork.UnitOfWork>();
            services.AddSingleton<ILoginAttemptUnitOfWorkFactory, LoginAttemptUnitOfWorkFactory>();

            // Current User
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ICurrentUserValidationService,
                CurrentUserValidationService>();

            return services;
        }
    }
}
