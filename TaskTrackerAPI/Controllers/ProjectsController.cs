// GET: api/projects:  Entity-based retrieval followed by mapping in the Service layer.
// GET: api/projects/projected: Direct DTO projection at the database level using ProjectTo<TDto>().


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
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await _projectService.GetAllAsync();

            return Ok(projects);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("projected")]
        public async Task<IActionResult> GetAllProjected()
        {
            var projects = await _projectService.GetAllProjectedAsync();
            return Ok(projects);
        }
        // GET: api/projects/{id}
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(ProjectDto projectDto)
        {
                var result = await _projectService.CreateAsync(projectDto);

                if (!result.Success)
                    return HandleError(result); 

                return CreatedAtAction(nameof(FindByID), new { Id = result.Data.Id }, result.Data);

        }
        // GET: api/projects/{id}
        [Authorize]
        [HttpGet("{Id}")]
        public async Task<IActionResult> FindByID(int Id)
        {
            if (Id <= 0)
                return BadRequest("Invalid Project ID");

            var project = await _projectService.FindByIDAsync(Id);
            if(project == null)
                return NotFound();
            
            return Ok(project);
        }
        // PUT: api/projects/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{Id}")]
        public async Task<IActionResult> Update(int Id, ProjectDto projectDto)
        {
                var result = await _projectService.UpdateAsync(Id, projectDto);

                if (!result.Success)
                    return HandleError(result);

                return NoContent();

        }

        // DELETE: api/projects/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            if (Id <= 0)
                return BadRequest("Invalid Project ID");

            var result = await _projectService.DeleteAsync(Id);

            if (!result.Success)
                return HandleError(result);

            return NoContent();
        }

    }
}
