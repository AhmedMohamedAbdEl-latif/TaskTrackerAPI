using System.ComponentModel.DataAnnotations;
// GET: api/comments:  Entity-based retrieval followed by mapping in the Service layer.
// GET: api/comments/projected: DTO projection directly in the database query.

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
        public async Task<IActionResult> GetAllComments(
            [FromQuery] PaginationQueryDto pagination,
            CancellationToken cancellationToken)
        {
            var comments = await commentService.ListAllAsync(pagination, cancellationToken);

            return Ok(SuccessResult(comments));
        }
        // GET: api/comments/projected
        [Authorize(Roles = "Admin")]
        [HttpGet("projected")]
        public async Task<IActionResult> GetAllProjected(
            [FromQuery] PaginationQueryDto pagination,
            CancellationToken cancellationToken)
        {
            var comments = await commentService.GetAllProjectedAsync(pagination, cancellationToken);
            return Ok(SuccessResult(comments));
        }

        // GET: api/comment/{id}
        [Authorize]
        [HttpGet("{Id}")]
        public async Task<IActionResult> FindByID([Range(1, int.MaxValue)] int Id, CancellationToken cancellationToken)
        {

            var result = await commentService.GetByIdAsync(Id, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(result);
        }

        // GET: api/comment/task/{taskId}
        [Authorize]
        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetCommentsByTaskId(
            [Range(1, int.MaxValue)] int taskId,
            [FromQuery] PaginationQueryDto pagination,
            CancellationToken cancellationToken)
        {

            var result = await commentService.GetByTaskIdAsync(taskId, pagination, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(result);
        }

        // PUT: api/comment
        [Authorize]
        [HttpPut("{Id}")]
        public async Task<IActionResult> Update([Range(1, int.MaxValue)] int Id, UpdateCommentDto commentDto, CancellationToken cancellationToken)
        {

            var result = await commentService.UpdateAsync(Id, commentDto, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(result);
        }

        // POST: api/comment
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateCommentDto commentDto, CancellationToken cancellationToken)
        {
                var result = await commentService.CreateAsync(commentDto, cancellationToken);

                if (!result.Success)
                    return HandleError(result);

                return CreatedAtAction(nameof(FindByID), new {Id = result.Data!.Id}, result);
        }

        // DELETE: api/comment/{id}
        [Authorize]
        [HttpDelete("{Id}")]
        public async Task<IActionResult> Delete([Range(1, int.MaxValue)] int Id, CancellationToken cancellationToken)
        {

            var result = await commentService.DeleteAsync(Id, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(SuccessResult());
        }
    }
}
