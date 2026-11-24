using System.ComponentModel.DataAnnotations;
// GET: api/projects:  Entity-based retrieval followed by mapping in the Service layer.
// GET: api/projects/projected: DTO projection directly in the database query.


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ApiControllerBase
    {
        // Dependency Injection
        private readonly IProjectService _projectService;

        // Constructor
        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }
        // GET: api/projects
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllProjects(
            [FromQuery] PaginationQueryDto pagination,
            CancellationToken cancellationToken)
        {
            var projects = await _projectService.GetAllAsync(pagination, cancellationToken);

            return Ok(SuccessResult(projects));
        }

        // GET: api/projects/projected
        [Authorize(Roles = "Admin")]
        [HttpGet("projected")]
        public async Task<IActionResult> GetAllProjected(
            [FromQuery] PaginationQueryDto pagination,
            CancellationToken cancellationToken)
        {
            var projects = await _projectService.GetAllProjectedAsync(pagination, cancellationToken);
            return Ok(SuccessResult(projects));
        }

        // GET: api/projects/{id}
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(ProjectDto projectDto, CancellationToken cancellationToken)
        {
            var result = await _projectService.CreateAsync(projectDto, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return CreatedAtAction(
                nameof(FindByID),
                new { Id = result.Data!.Id },
                result);

        }

        // GET: api/projects/{id}
        [Authorize]
        [HttpGet("{Id}")]
        public async Task<IActionResult> FindByID([Range(1, int.MaxValue)] int Id, CancellationToken cancellationToken)
        {

            var project = await _projectService.FindByIDAsync(Id, cancellationToken);
            if (project == null)
                return NotFoundError("Project not found.");

            return Ok(SuccessResult(project));
        }

        // PUT: api/projects/{id}
        [Authorize]
        [HttpPut("{Id}")]
        public async Task<IActionResult> Update([Range(1, int.MaxValue)] int Id, ProjectDto projectDto, CancellationToken cancellationToken)
        {
            var result = await _projectService.UpdateAsync(Id, projectDto, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(SuccessResult());

        }

        // DELETE: api/projects/{id}
        [Authorize]
        [HttpDelete("{Id}")]
        public async Task<IActionResult> Delete([Range(1, int.MaxValue)] int Id, CancellationToken cancellationToken)
        {

            var result = await _projectService.DeleteAsync(Id, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(SuccessResult());
        }

    }
}
