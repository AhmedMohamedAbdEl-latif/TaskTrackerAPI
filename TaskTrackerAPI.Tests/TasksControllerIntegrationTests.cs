
using System.Net;
using TaskTrackerAPI.Tests.Helpers;

namespace TaskTrackerAPI.Tests
{
    public class TasksControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {

        private readonly HttpClient _client;

        public TasksControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();

        }

        [Fact]

        public async Task GetTask_InvalidId_ReturnsBadRequest()
        {
            var response = await _client.GetAsync("/api/tasks/0");

            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine($"Response URI: {response.RequestMessage?.RequestUri}");
            Console.WriteLine($"Response Headers: {response.Headers}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetTasks_ReturnsExpectedResponse()
        {
            var response = await _client.GetAsync("/api/tasks");

            Console.WriteLine($"Status Code: {response.StatusCode}");

            Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
