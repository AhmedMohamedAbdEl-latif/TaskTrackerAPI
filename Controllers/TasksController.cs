using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TaskTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ApiControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IUnitOfWork _unitOfWork;

        public TasksController(ITaskService taskService, IUnitOfWork unitOfWork)
        {
            _taskService = taskService;
            _unitOfWork = unitOfWork;
        }
        // GET: api/tasks
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await _taskService.ListAllAsync();
            return Ok(tasks);
        }
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
            var task = await _taskService.FindByIDAsync(Id);
            if (task == null)
                return NotFound();

            return Ok(task);
        }
        // POST: api/tasks
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(TaskDto taskDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskService.CreateAsync(taskDto);

                if (!result.Success)
                    return HandleError(result);

                await _unitOfWork.SaveChangesAsync();

                return CreatedAtAction(nameof(FindByID), new { id = result.Data.Id }, result.Data);
                // return Ok(result.Data); 

            }
            return BadRequest(ModelState);
        }
        // PUT: api/tasks/{Id}
        [Authorize]
        [HttpPut("{Id}")]
        public async Task<IActionResult> Update(int Id, TaskDto taskDto)
        {
            if (ModelState.IsValid)
            {

                var userIdClaims = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaims == null)
                    return Unauthorized();

                var currentUserId = int.Parse(userIdClaims.Value);
                var isAdmin = User.IsInRole("Admin");

                var result = await _taskService.UpdateAsync(Id, taskDto, currentUserId, isAdmin);

                if (!result.Success)
                    return HandleError(result);

                await _unitOfWork.SaveChangesAsync();

                return NoContent();

            }
            return BadRequest(ModelState);
        }
        // DELETE: api/tasks/{Id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            if (Id <= 0)
                return BadRequest("Invalid Task ID");
            var task = await _taskService.FindByIDAsync(Id);

            if (task == null)
                return NotFound();

            _taskService.Delete(Id);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
        // GET: api/tasks/project/{projectId}
        [Authorize]
        [HttpGet("project/{projectId}")]
        public IActionResult GetTasksByProjectId(int projectId)
        {
            var tasks = _taskService.GetByProjectID(projectId);

            return Ok(tasks);
        }
        // GET: api/tasks/user/{userId}
        [Authorize]
        [HttpGet("user/{userId}")]
        public IActionResult GetTasksByUserId(int userId)
        {
            var tasks = _taskService.GetByUserID(userId);

            return Ok(tasks);
        }
        // GET: api/tasks/status/{status}
        [Authorize]
        [HttpGet("status/{status}")]
        public IActionResult GetTasksByStatus(ProjectTaskStatus status)
        {
            var tasks = _taskService.GetByStatus(status);

            return Ok(tasks);
        }
    }
}
