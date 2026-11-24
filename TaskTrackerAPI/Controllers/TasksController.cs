using System.ComponentModel.DataAnnotations;
// GET: api/tasks:  Entity-based retrieval followed by mapping in the Service layer.
// GET: api/tasks/projected: DTO projection directly in the database query.



using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerAPI.DTO.Pagination;

namespace TaskTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ApiControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }
        // GET: api/tasks
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllTasks(
            [FromQuery] PaginationQueryDto pagination,
            CancellationToken cancellationToken)
        {
            var result = await _taskService.ListAllAsync(pagination, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("projected")]
        public async Task<IActionResult> GetAllProjected(
            [FromQuery] PaginationQueryDto pagination,
            CancellationToken cancellationToken)
        {
            var tasks = await _taskService.ListAllProjectedAsync(pagination, cancellationToken);
            return Ok(SuccessResult(tasks));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
        {
            var stats = await _taskService.GetStatsAsync(cancellationToken);

            return Ok(SuccessResult(stats));
        }

        // GET: api/tasks/{Id}
        [Authorize]
        [HttpGet("{Id}")]
        public async Task<IActionResult> FindByID([Range(1, int.MaxValue)] int Id, CancellationToken cancellationToken)
        {

            var result = await _taskService.FindByIDAsync(Id, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] PaginationQueryDto pagination,
            CancellationToken cancellationToken)
        {
            var result = await _taskService.GetPagedAsync(pagination, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(result);
        }


        // POST: api/tasks
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(TaskDto taskDto, CancellationToken cancellationToken)
        {
            var result = await _taskService.CreateAsync(taskDto, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return CreatedAtAction(nameof(FindByID), new { id = result.Data!.Id }, result);

        }
        // PUT: api/tasks/{Id}
        [Authorize]
        [HttpPut("{Id}")]
        public async Task<IActionResult> Update([Range(1, int.MaxValue)] int Id, TaskDto taskDto, CancellationToken cancellationToken)
        {

            var result = await _taskService.UpdateAsync(Id, taskDto, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(SuccessResult());
        }

        // PATCH: api/tasks/{Id}/status
        [Authorize]
        [HttpPatch("{Id}/status")]
        public async Task<IActionResult> UpdateStatus([Range(1, int.MaxValue)] int Id, UpdateTaskStatusDto statusDto, CancellationToken cancellationToken)
        {

            var result = await _taskService.UpdateStatusAsync(Id, statusDto, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(SuccessResult());
        }

        // DELETE: api/tasks/{Id}
        [Authorize]
        [HttpDelete("{Id}")]
        public async Task<IActionResult> Delete([Range(1, int.MaxValue)] int Id, CancellationToken cancellationToken)
        {

            var result = await _taskService.DeleteAsync(Id, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(SuccessResult());
        }
        // GET: api/tasks/project/{projectId}
        [Authorize]
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetTasksByProjectId(
            [Range(1, int.MaxValue)] int projectId,
            [FromQuery] PaginationQueryDto pagination,
            CancellationToken cancellationToken)
        {

            var result = await _taskService.GetByProjectID(projectId, pagination, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(result);
        }

        // GET: api/tasks/user/{userId}
        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetTasksByUserId(
            [Range(1, int.MaxValue)] int userId,
            [FromQuery] PaginationQueryDto pagination,
            CancellationToken cancellationToken)
        {

            var result = await _taskService.GetTasksByUserAsync(userId, pagination, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(result);
        }

        // GET: api/tasks/status/{status}
        [Authorize]
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetTasksByStatus(
            [EnumDataType(typeof(ProjectTaskStatus))] ProjectTaskStatus status,
            [FromQuery] PaginationQueryDto pagination,
            CancellationToken cancellationToken)
        {
            var result = await _taskService.GetByStatus(status, pagination, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(result);
        }
    }
}
