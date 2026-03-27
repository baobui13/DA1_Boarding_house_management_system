using Backend_Boarding_house_management_system.DTOs.User.Requests;
using Backend_Boarding_house_management_system.DTOs.User.Responses;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;
using Backend_Boarding_house_management_system.Entities;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("GetUserByIdOrEmail")]
        public async Task<ActionResult<UserResponse>> GetUserByIdOrEmail([FromQuery] GetUserByIdOrEmailRequest request)
        {
            var user = await _userService.GetUserByIdOrEmailAsync(request);
            return Ok(user);
        }

        [HttpGet("GetUsersByFilter")]
        public async Task<ActionResult<UserListResponse>> GetUsersByFilter(
            [FromQuery] EntityFilter<User> filter,
            [FromQuery] EntitySort<User> sort,
            [FromQuery] EntityPage page)
        {
            var users = await _userService.GetUsersByFilterAsync(filter, sort, page);
            return Ok(users);
        }

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            await _userService.UpdateUserAsync(request);
            return Ok();
        }

        [HttpPut("UpdateUserAvatar")]
        public async Task<IActionResult> UpdateUserAvatar([FromBody] UpdateUserAvatarRequest request)
        {
            await _userService.UpdateUserAvatarAsync(request);
            return Ok();
        }

        [HttpPut("BlockUser")]
        public async Task<IActionResult> BlockUser([FromBody] BlockUserRequest request)
        {
            await _userService.BlockUserAsync(request);
            return Ok();
        }

        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserRequest request)
        {
            await _userService.DeleteUserAsync(request);
            return Ok();
        }
    }
}