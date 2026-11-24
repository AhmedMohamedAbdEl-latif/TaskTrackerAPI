using Moq;
using TaskTrackerAPI.Common;
using TaskTrackerAPI.DTO.Pagination;
using TaskTrackerAPI.Repository;
using TaskTrackerAPI.Services;
using TaskTrackerAPI.UnitOfWork;
using ProjectTaskStatus = TaskTrackerAPI.Models.TaskStatus;

namespace TaskTrackerAPI.Tests.Services
{
    public class TaskServiceTests
    {
        [Fact]
        public void IsValidStatusTransition_ToDoToInProgress_ReturnsTrue()
        {
            // Arrange
            var currentStatus = ProjectTaskStatus.ToDo;
            var newStatus = ProjectTaskStatus.InProgress;
            // Act
            var result = TaskService.IsValidStatusTransition(currentStatus, newStatus);
            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(ProjectTaskStatus.ToDo, ProjectTaskStatus.Done)]
        [InlineData(ProjectTaskStatus.Done, ProjectTaskStatus.InProgress)]
        public void IsValidStatusTransition_InvalidTransitions_ReturnsFalse(ProjectTaskStatus currentStatus, ProjectTaskStatus newStatus)
        {
            // Act
            var result = TaskService.IsValidStatusTransition(currentStatus, newStatus);
            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetPagedAsync_NonAdminUsers_ReturnsForbidden()
        {
            var taskRepository = new Mock<ITaskRepository>();
            var projectRepository = new Mock<IProjectRepository>();
            var userRepository = new Mock<IUserRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var currentUser = new Mock<ICurrentUserService>();
            var comment = new Mock<ICommentRepository>();

            currentUser.Setup(c => c.IsAdmin).Returns(false);

            var taskService = new TaskService(
                taskRepository.Object,
                projectRepository.Object,
                userRepository.Object,
                unitOfWork.Object,
                currentUser.Object,
                comment.Object
                );
            var pagination = new PaginationQueryDto
            {
                PageNumber = 1,
                PageSize = 10
            };

            var result = await taskService.GetPagedAsync(pagination);

            Assert.False(result.Success);
            Assert.Equal(ServiceErrorType.Forbidden, result.ErrorType);
        }
    }
}
