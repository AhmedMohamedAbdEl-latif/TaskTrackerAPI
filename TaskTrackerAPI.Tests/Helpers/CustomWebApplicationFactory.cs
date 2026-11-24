using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using TaskTrackerAPI.Common;
using TaskTrackerAPI.DTO;
using TaskTrackerAPI.DTO.Pagination;
using TaskTrackerAPI.Services;
using TaskTrackerAPI.Services.Auth;

namespace TaskTrackerAPI.Tests.Helpers
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Keep the test host in Development so it uses the development-only
            // Data Protection setup, then replace every secret/DB setting below.
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:TaskTrackerDatabase"] =
                        "Server=127.0.0.1,1;Database=TaskTrackerApiTests;User Id=test;Password=test;Encrypt=False;Connect Timeout=1",
                    ["AdminBootstrap:Name"] = string.Empty,
                    ["AdminBootstrap:Email"] = string.Empty,
                    ["AdminBootstrap:Password"] = string.Empty,
                    ["JWT:SecretKey"] = "TaskTracker.Tests.Only.Dummy.Jwt.Secret.Key.1234567890",
                    ["JWT:IssuerIP"] = "TaskTracker.Tests",
                    ["JWT:AudienceIP"] = "TaskTracker.Tests.Client",
                    ["JWT:DurationInMinutes"] = "30",
                    ["DataProtection:KeysPath"] = Path.Combine(
                        Path.GetTempPath(),
                        "TaskTrackerAPI.Tests.DataProtection")
                });
            });

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IAuthenticationSchemeProvider>();
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                    options.DefaultScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

                services.RemoveAll<ITaskService>();
                var taskService = new Mock<ITaskService>();
                taskService
                    .Setup(service => service.ListAllAsync(
                        It.IsAny<PaginationQueryDto>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new ServiceResult<PagedResultDto<TaskResponseDto>>
                    {
                        Success = true,
                        Data = new PagedResultDto<TaskResponseDto>
                        {
                            Items = new List<TaskResponseDto>(),
                            TotalCount = 0,
                            PageNumber = 1,
                            PageSize = 20
                        }
                    });
                services.AddSingleton(taskService.Object);

                var retentionService = services.FirstOrDefault(descriptor =>
                    descriptor.ServiceType == typeof(IHostedService) &&
                    descriptor.ImplementationType == typeof(LoginAttemptRetentionService));
                if (retentionService is not null)
                    services.Remove(retentionService);
            });
        }
    }
}
