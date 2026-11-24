using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ApiControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUser;

        public UsersController(
            IUserService userService,
            ICurrentUserService currentUser)
        {
            _userService = userService;
            _currentUser = currentUser;
        }

        // GET: api/users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] PaginationQueryDto pagination,
            CancellationToken cancellationToken)
        {
            var users = await _userService.ListAllAsync(pagination, cancellationToken);

            return Ok(SuccessResult(users));
        }

        // GET: api/users/projected
        [Authorize(Roles = "Admin")]
        [HttpGet("projected")]
        public async Task<IActionResult> GetAllProjected(
            [FromQuery] PaginationQueryDto pagination,
            CancellationToken cancellationToken)
        {
            var users = await _userService.GetAllProjectedAsync(pagination, cancellationToken);

            return Ok(SuccessResult(users));
        }

        // GET: api/users/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> FindByID([Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAdmin && _currentUser.UserId != id)
                return ForbiddenError();

            var user = await _userService.FindByIDAsync(id, cancellationToken);

            if (user == null)
                return NotFoundError("User not found.");

            return Ok(SuccessResult(user));
        }

        // PUT: api/users/{id}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            [Range(1, int.MaxValue)] int id,
            UserDto userDto,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAdmin && _currentUser.UserId != id)
                return ForbiddenError();

            var result = await _userService.UpdateAsync(
                id, userDto, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(SuccessResult());
        }

        // PATCH: api/users/{id}/role
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/role")]
        public async Task<IActionResult> UpdateRole(
            [Range(1, int.MaxValue)] int id,
            UpdateUserRoleDto roleDto,
            CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateRoleAsync(
                id, roleDto, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(SuccessResult());
        }

        // DELETE: api/users/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            [Range(1, int.MaxValue)] int id,
            CancellationToken cancellationToken)
        {

            var result = await _userService.DeleteAsync(
                id, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return Ok(SuccessResult());
        }
    }
}
