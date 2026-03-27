using Backend_Boarding_house_management_system.DTOs.RoomAmenity.Requests;
using Backend_Boarding_house_management_system.DTOs.RoomAmenity.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;
using Backend_Boarding_house_management_system.Entities;

using Backend_Boarding_house_management_system.Services.Interfaces;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoomAmenityController : ControllerBase
    {
        private readonly IRoomAmenityService _roomAmenityService;

        public RoomAmenityController(IRoomAmenityService roomAmenityService)
        {
            _roomAmenityService = roomAmenityService;
        }
        [AllowAnonymous]
        [HttpGet("GetRoomAmenityById")]
        public async Task<ActionResult<RoomAmenityResponse>> GetRoomAmenityById([FromQuery] GetRoomAmenityByIdRequest request)
        {
            var result = await _roomAmenityService.GetByIdAsync(request);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("GetRoomAmenitiesByFilter")]
        public async Task<ActionResult<RoomAmenityListResponse>> GetRoomAmenitiesByFilter(
            [FromQuery] EntityFilter<RoomAmenity> filter,
            [FromQuery] EntitySort<RoomAmenity> sort,
            [FromQuery] EntityPage page)
        {
            var result = await _roomAmenityService.GetByFilterAsync(filter, sort, page);
            return Ok(result);
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpPost("CreateRoomAmenity")]
        public async Task<ActionResult<RoomAmenityResponse>> CreateRoomAmenity([FromBody] CreateRoomAmenityRequest request)
        {
            var result = await _roomAmenityService.CreateAsync(request);
            return Ok(result);
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpPut("UpdateRoomAmenity")]
        public async Task<IActionResult> UpdateRoomAmenity([FromBody] UpdateRoomAmenityRequest request)
        {
            await _roomAmenityService.UpdateAsync(request);
            return Ok();
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpDelete("DeleteRoomAmenity")]
        public async Task<IActionResult> DeleteRoomAmenity([FromBody] DeleteRoomAmenityRequest request)
        {
            await _roomAmenityService.DeleteAsync(request);
            return Ok();
        }
    }
}
