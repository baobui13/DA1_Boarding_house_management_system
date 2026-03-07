using Backend_Boarding_house_management_system.DTOs.User.Requests;
using Backend_Boarding_house_management_system.DTOs.User.Responses;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetUserByIdOrEmail([FromQuery] GetUserByIdOrEmailRequest request)
        {
            var user = await _userService.GetUserByIdOrEmailAsync(request);
            return Ok(user);
        }

        [HttpGet("GetUsersByFilter")]
        public async Task<IActionResult> GetUsersByFilter([FromQuery] GetUsersByFilterRequest request)
        {
            var users = await _userService.GetUsersByFilterAsync(request);
            return Ok(users);
        }

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            await _userService.UpdateUserAsync(request);
            return NoContent();
        }

        [HttpPut("UpdateUserAvatar")]
        public async Task<IActionResult> UpdateUserAvatar([FromBody] UpdateUserAvatarRequest request)
        {
            await _userService.UpdateUserAvatarAsync(request);
            return NoContent();
        }

        [HttpPut("BlockUser")]
        public async Task<IActionResult> BlockUser([FromBody] BlockUserRequest request)
        {
            await _userService.BlockUserAsync(request);
            return NoContent();
        }

        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserRequest request)
        {
            await _userService.DeleteUserAsync(request);
            return NoContent();
        }
    }
}