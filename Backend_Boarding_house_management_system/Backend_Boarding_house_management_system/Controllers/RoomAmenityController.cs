using Backend_Boarding_house_management_system.DTOs.RoomAmenity.Requests;
using Backend_Boarding_house_management_system.DTOs.RoomAmenity.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomAmenityController : ControllerBase
    {
        [HttpGet("GetRoomAmenityById")]
        public async Task<ActionResult<RoomAmenityResponse>> GetRoomAmenityById([FromQuery] GetRoomAmenityByIdRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpGet("GetRoomAmenitiesByFilter")]
        public async Task<ActionResult<RoomAmenityListResponse>> GetRoomAmenitiesByFilter([FromQuery] GetRoomAmenitiesByFilterRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("CreateRoomAmenity")]
        public async Task<ActionResult<RoomAmenityResponse>> CreateRoomAmenity([FromBody] CreateRoomAmenityRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPut("UpdateRoomAmenity")]
        public async Task<IActionResult> UpdateRoomAmenity([FromBody] UpdateRoomAmenityRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("DeleteRoomAmenity")]
        public async Task<IActionResult> DeleteRoomAmenity([FromBody] DeleteRoomAmenityRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
