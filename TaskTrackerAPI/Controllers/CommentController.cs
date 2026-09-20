using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TaskTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ApiControllerBase
    {
        private readonly ICommentService commentService;
        private readonly IUnitOfWork unitOfWork;

        public CommentController(ICommentService _commentService, IUnitOfWork _unitOfWork)
        {
            commentService = _commentService;
            unitOfWork = _unitOfWork;
        }
        // GET: api/comment
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllComments()
        {
            var comments = await commentService.ListAllAsync();

            return Ok(comments);
        }
        // GET: api/comment/{id}
        [Authorize]
        [HttpGet("{Id}")]
        public async Task<IActionResult> FindByID(int Id)
        {
            var comment = await commentService.GetByIdAsync(Id);
            if (comment == null)
                return NotFound();

            return Ok(comment);
        }
        // GET: api/comment/task/{taskId}
        [Authorize]
        [HttpGet("task/{taskId}")]
        public IActionResult GetCommentsByTaskId(int taskId)
        {
            var comments = commentService.GetByTaskId(taskId);

            return Ok(comments);
        }
        // PUT: api/comment
        [Authorize]
        [HttpPut("{Id}")]
        public async Task<IActionResult> Update(int Id, UpdateCommentDto commentDto)
        {
            if (ModelState.IsValid)
            {
               var result = await commentService.UpdateAsync(Id, commentDto);

                if (!result.Success)
                    return HandleError(result);

                await unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            return BadRequest(ModelState);
        }
        // POST: api/comment
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateCommentDto commentDto)
        {
            if (ModelState.IsValid)
            {
                var result = await commentService.CreateAsync(commentDto);

                if (!result.Success)
                    return HandleError(result);

                await unitOfWork.SaveChangesAsync();

                return CreatedAtAction(nameof(FindByID), new {Id = result.Data.Id}, result.Data);

            }
            return BadRequest(ModelState);
        }
        // DELETE: api/comment/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            if(Id <= 0)
                return BadRequest("Invalid Comment ID");  
            
            var comment = await commentService.GetByIdAsync(Id);

            if (comment == null)
                return NotFound();

            commentService.Delete(Id);
            await unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}
