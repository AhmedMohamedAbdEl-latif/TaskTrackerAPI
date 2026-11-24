using System.Net;
using System.Net.Http.Json;
using TaskTrackerAPI.Common;
using TaskTrackerAPI.DTO;
using TaskTrackerAPI.DTO.Pagination;
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
        public async Task GetTask_InvalidId_ReturnsValidationEnvelope()
        {
            var response = await _client.GetAsync("/api/tasks/0");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<ServiceResult>();
            Assert.NotNull(body);
            Assert.False(body!.Success);
            Assert.Equal(ServiceErrorType.Validation, body.ErrorType);
            Assert.NotEmpty(body.Errors);
        }

        [Fact]
        public async Task GetTasks_ReturnsSuccessfulPagedResponseEnvelope()
        {
            var response = await _client.GetAsync("/api/tasks?pageNumber=1&pageSize=20");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content
                .ReadFromJsonAsync<ServiceResult<PagedResultDto<TaskResponseDto>>>();

            Assert.NotNull(body);
            Assert.True(body!.Success);
            Assert.NotNull(body.Data);
            Assert.Empty(body.Data!.Items);
            Assert.Equal(0, body.Data.TotalCount);
            Assert.Equal(1, body.Data.PageNumber);
            Assert.Equal(20, body.Data.PageSize);
            Assert.Equal(0, body.Data.TotalPages);
        }
    }
}
