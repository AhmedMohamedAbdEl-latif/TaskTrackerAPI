using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TaskTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ApiControllerBase
    {
        private readonly IUserService userService;
        private readonly IUnitOfWork unitOfWork;

        public UsersController(IUserService _userService, IUnitOfWork _unitOfWork)
        {
            userService = _userService;
            unitOfWork = _unitOfWork;
        }
        // GET: api/users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await userService.ListAllAsync();

            return Ok(users);
        }
        // GET: api/users/{id}
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> FindByID(int id)
        {
            var user = await userService.FindByIDAsync(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }
        // POST: api/users
        //[HttpPost]
        //public async Task<IActionResult> Create(UserDto userDto)
        //{
        //    if (ModelState.IsValid)
        //    {
        //       var result = await userService.CreateAsync(userDto);

        //        if (!result.Success)
        //            return HandleError(result);

        //        await unitOfWork.SaveChangesAsync();

        //        return CreatedAtAction(nameof(FindByID), new { Id = result.Data.Id }, result.Data);
        //    }

        //    return BadRequest(ModelState);
        // }


        // PUT: api/users/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UserDto userDto)
        {
            if (ModelState.IsValid)
            {
                var result = await userService.UpdateAsync(id, userDto);

                if(!result.Success)
                    return HandleError(result);

                await unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            return BadRequest(ModelState);
        }
        // DELETE: api/users/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid user ID");

            var user = await userService.FindByIDAsync(id);

            if (user == null)
                return NotFound();

            userService.Delete(id);
            await unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}
