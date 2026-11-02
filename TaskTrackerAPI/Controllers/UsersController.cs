// GET: api/Users:  Entity-based retrieval followed by mapping in the Service layer.
// GET: api/Users/projected: Direct DTO projection at the database level using ProjectTo<TDto>().

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ApiControllerBase
    {
        private readonly IUserService userService;
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentUserService _currentUser;
        public UsersController(IUserService _userService, IUnitOfWork _unitOfWork, ICurrentUserService currentUser)
        {
            userService = _userService;
            unitOfWork = _unitOfWork;
            _currentUser = currentUser;
        }
        // GET: api/users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await userService.ListAllAsync();

            return Ok(users);
        }

        // GET: api/users/projected
        [Authorize(Roles = "Admin")]
        [HttpGet("projected")]
        public async Task<IActionResult> GetAllProjected()
        {
            var users = await userService.GetAllProjectedAsync();
            return Ok(users);
        }

        // GET: api/users/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> FindByID(int id)
        {
            if (!_currentUser.IsAdmin && _currentUser.UserId != id)
                return Forbid();

            var user = await userService.FindByIDAsync(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }
        // POST: api/users
        //[HttpPost]
        //public async Task<IActionResult> Create(UserDto userDto)
        //{
        //    
        //       var result = await userService.CreateAsync(userDto);

        //        if (!result.Success)
        //            return HandleError(result);

        //        await unitOfWork.SaveChangesAsync();

        //        return CreatedAtAction(nameof(FindByID), new { Id = result.Data.Id }, result.Data);
        // }


        // PUT: api/users/{id}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UserDto userDto)
        {
            if (!_currentUser.IsAdmin && _currentUser.UserId != id)
                return Forbid();

            var result = await userService.UpdateAsync(id, userDto);

                if(!result.Success)
                    return HandleError(result);

                await unitOfWork.SaveChangesAsync();

            return NoContent();
            
        }
        // DELETE: api/users/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid user ID");

            var result = await userService.DeleteAsync(id);

            if (!result.Success)
                return HandleError(result);

            await unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}
