// GET: api/comments:  Entity-based retrieval followed by mapping in the Service layer.
// GET: api/comments/projected: Direct DTO projection at the database level using ProjectTo<TDto>().

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ApiControllerBase
    {
        private readonly ICommentService commentService;

        public CommentsController(ICommentService _commentService)
        {
            commentService = _commentService;
        }
        // GET: api/comments
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllComments()
        {
            var comments = await commentService.ListAllAsync();

            return Ok(comments);
        }
        // GET: api/comments/projected
        [Authorize(Roles = "Admin")]
        [HttpGet("projected")]
        public async Task<IActionResult> GetAllProjected()
        {
            var comments = await commentService.GetAllProjectedAsync();
            return Ok(comments);
        }

        // GET: api/comment/{id}
        [Authorize]
        [HttpGet("{Id}")]
        public async Task<IActionResult> FindByID(int Id)
        {
            var result = await commentService.GetByIdAsync(Id);

            if (!result.Success)
                return HandleError(result);

            return Ok(result.Data);
        }

        // GET: api/comment/task/{taskId}
        [Authorize]
        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetCommentsByTaskId(int taskId)
        {
            var result = await commentService.GetByTaskIdAsync(taskId);

            if (!result.Success)
                return HandleError(result);

            return Ok(result.Data);
        }

        // PUT: api/comment
        [Authorize]
        [HttpPut("{Id}")]
        public async Task<IActionResult> Update(int Id, UpdateCommentDto commentDto)
        {
            var result = await commentService.UpdateAsync(Id, commentDto);

            if (!result.Success)
                return HandleError(result);

            return Ok(result.Data);
        }

        // POST: api/comment
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateCommentDto commentDto)
        {
                var result = await commentService.CreateAsync(commentDto);

                if (!result.Success)
                    return HandleError(result);

                return CreatedAtAction(nameof(FindByID), new {Id = result.Data.Id}, result.Data);
        }

        // DELETE: api/comment/{id}
        [Authorize]
        [HttpDelete("{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            if (Id <= 0)
                return BadRequest("Invalid Comment ID");

            var result = await commentService.DeleteAsync(Id);

            if (!result.Success)
                return HandleError(result);

            return NoContent();
        }
    }
}
