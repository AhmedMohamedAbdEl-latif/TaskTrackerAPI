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
        private readonly IUnitOfWork _unitOfWork;
        // Constructor
        public ProjectsController(IProjectService projectService, IUnitOfWork unitOfWork)
        {
            _projectService = projectService;
            _unitOfWork = unitOfWork;
        }
        // GET: api/projects
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await _projectService.GetAllAsync();

            return Ok(projects);
        }
        // GET: api/projects/{id}
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(ProjectDto projectDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _projectService.CreateAsync(projectDto);

                if (!result.Success)
                    return HandleError(result);
                
                await _unitOfWork.SaveChangesAsync();

                return CreatedAtAction(nameof(FindByID), new { Id = result.Data.Id }, result.Data);

            }
            return BadRequest(ModelState);
        }
        // GET: api/projects/{id}
        [Authorize]
        [HttpGet("{Id}")]
        public async Task<IActionResult> FindByID(int Id)
        {
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
            if (ModelState.IsValid)
            {
                var result = await _projectService.UpdateAsync(Id, projectDto);

                if (!result.Success)
                    return HandleError(result);

                await _unitOfWork.SaveChangesAsync();

                return NoContent();

            }
            return BadRequest(ModelState);
        }
        // DELETE: api/projects/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            if(Id <= 0)
                return BadRequest("Invalid Project ID");

            var project = await _projectService.FindByIDAsync(Id);

            if(project == null)
                return NotFound();

            _projectService.Delete(Id);

            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

    }
}
