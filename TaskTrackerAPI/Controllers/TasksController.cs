// GET: api/tasks:  Entity-based retrieval followed by mapping in the Service layer.
// GET: api/tasks/projected: Direct DTO projection at the database level using ProjectTo<TDto>().



using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetAllTasks()
        {
            var result = await _taskService.ListAllAsync();

            if (!result.Success)
                return HandleError(result);

            return Ok(result.Data);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("projected")]
        public async Task<IActionResult> GetAllProjected()
        {
            var tasks = await _taskService.ListAllProjectedAsync();
            return Ok(tasks);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _taskService.GetStatsAsync();

            return Ok(stats);
        }

        // GET: api/tasks/{Id}
        [Authorize]
        [HttpGet("{Id}")]
        public async Task<IActionResult> FindByID(int Id)
        {
            var result = await _taskService.FindByIDAsync(Id);

            if (!result.Success)
                return HandleError(result);

            return Ok(result.Data);
        }
        // POST: api/tasks
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(TaskDto taskDto)
        {
                var result = await _taskService.CreateAsync(taskDto);

                if (!result.Success)
                    return HandleError(result);

                return CreatedAtAction(nameof(FindByID), new { id = result.Data.Id }, result.Data);
               
        }
        // PUT: api/tasks/{Id}
        [Authorize]
        [HttpPut("{Id}")]
        public async Task<IActionResult> Update(int Id, TaskDto taskDto)
        {
            var result = await _taskService.UpdateAsync(Id, taskDto);

            if (!result.Success)
                return HandleError(result);

            return NoContent();
        }

        // DELETE: api/tasks/{Id}
        [Authorize]
        [HttpDelete("{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            if (Id <= 0)
                return BadRequest("Invalid Task ID");

           var result = await _taskService.DeleteAsync(Id);

            if (!result.Success)
                return HandleError(result);

            return NoContent();
        }
        // GET: api/tasks/project/{projectId}
        [Authorize]
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetTasksByProjectId(int projectId)
        {
            var result = await _taskService.GetByProjectID(projectId);

            if (!result.Success)
                return HandleError(result);

            return Ok(result.Data);
        }

        // GET: api/tasks/user/{userId}
        [Authorize]
        [HttpGet("user/{userId}")]
        public IActionResult GetTasksByUserId(int userId)
        {
            var result = _taskService.GetTasksByUserAsync(userId);

            if (!result.Success)
                return HandleError(result);

            return Ok(result.Data);
        }

        // GET: api/tasks/status/{status}
        [Authorize]
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetTasksByStatus(ProjectTaskStatus status)
        {
            var result = await _taskService.GetByStatus(status);

            if (!result.Success)
                return HandleError(result);

            return Ok(result.Data);
        }
    }
}
