using Backend_Boarding_house_management_system.DTOs.Area.Requests;
using Backend_Boarding_house_management_system.DTOs.Area.Responses;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AreaController : ControllerBase
    {
        private readonly IAreaService _areaService;
        public AreaController(IAreaService areaService)
        {
            _areaService = areaService;
        }

        [HttpGet("GetAreaByIdOrName")]
        public async Task<ActionResult<AreaResponse>> GetAreaByIdOrName([FromQuery] GetAreaByIdRequest request)
        {
            var area = await _areaService.GetAreaByIdAsync(request);
            return Ok(area);
        }

        [HttpGet("GetAreasByFilter")]
        public async Task<ActionResult<AreaListResponse>> GetAreasByFilter([FromQuery] GetAreasByFilterRequest request)
        {
            var areas = await _areaService.GetAreasByFilterAsync(request);
            return Ok(areas);
        }

        [HttpPost("CreateArea")]
        public async Task<ActionResult<AreaResponse>> CreateArea([FromBody] CreateAreaRequest request)
        {
            var area = await _areaService.CreateAreaAsync(request);
            return Ok(area);
        }

        [HttpPut("UpdateArea")]
        public async Task<IActionResult> UpdateArea([FromBody] UpdateAreaRequest request)
        {
            await _areaService.UpdateAreaAsync(request);
            return Ok();
        }

        [HttpPut("UpdateAreaDescription")]
        public async Task<IActionResult> UpdateAreaDescription([FromBody] UpdateAreaDescriptionRequest request)
        {
            await _areaService.UpdateAreaDescriptionAsync(request);
            return Ok();
        }

        [HttpDelete("DeleteArea")]
        public async Task<IActionResult> DeleteArea([FromBody] DeleteAreaRequest request)
        {
            await _areaService.DeleteAreaAsync(request);
            return Ok();
        }
    }
}
