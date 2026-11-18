
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
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.ListAllAsync();

            return Ok(users);
        }

        // GET: api/users/projected
        [Authorize(Roles = "Admin")]
        [HttpGet("projected")]
        public async Task<IActionResult> GetAllProjected()
        {
            var users = await _userService.GetAllProjectedAsync();

            return Ok(users);
        }

        // GET: api/users/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> FindByID(int id)
        {
            if (!_currentUser.IsAdmin && _currentUser.UserId != id)
                return ForbiddenError();

            var user = await _userService.FindByIDAsync(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // PUT: api/users/{id}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            UserDto userDto,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAdmin && _currentUser.UserId != id)
                return ForbiddenError();

            var result = await _userService.UpdateAsync(
                id, userDto, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return NoContent();
        }

        // PATCH: api/users/{id}/role
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/role")]
        public async Task<IActionResult> UpdateRole(
            int id,
            UpdateUserRoleDto roleDto,
            CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateRoleAsync(
                id, roleDto, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return NoContent();
        }

        // DELETE: api/users/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                var validationResult = new ServiceResult
                {
                    Success = false,
                    ErrorType = ServiceErrorType.Validation,
                    Errors = new List<string>
                    {
                        "Invalid user ID"
                    }
                };

                return HandleError(validationResult);
            }

            var result = await _userService.DeleteAsync(
                id, cancellationToken);

            if (!result.Success)
                return HandleError(result);

            return NoContent();
        }
    }
}
