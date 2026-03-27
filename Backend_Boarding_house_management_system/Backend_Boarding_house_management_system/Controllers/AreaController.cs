using Backend_Boarding_house_management_system.DTOs.Area.Requests;
using Backend_Boarding_house_management_system.DTOs.Area.Responses;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;
using Backend_Boarding_house_management_system.Entities;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AreaController : ControllerBase
    {
        private readonly IAreaService _areaService;
        public AreaController(IAreaService areaService)
        {
            _areaService = areaService;
        }

        [AllowAnonymous]
        [HttpGet("GetAreaByIdOrName")]
        public async Task<ActionResult<AreaResponse>> GetAreaByIdOrName([FromQuery] GetAreaByIdRequest request)
        {
            var area = await _areaService.GetAreaByIdAsync(request);
            return Ok(area);
        }

        [AllowAnonymous]
        [HttpGet("GetAreasByFilter")]
        public async Task<ActionResult<AreaListResponse>> GetAreasByFilter(
            [FromQuery] EntityFilter<Area> filter,
            [FromQuery] EntitySort<Area> sort,
            [FromQuery] EntityPage page)
        {
            var areas = await _areaService.GetAreasByFilterAsync(filter, sort, page);
            return Ok(areas);
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpPost("CreateArea")]
        public async Task<ActionResult<AreaResponse>> CreateArea([FromBody] CreateAreaRequest request)
        {
            var area = await _areaService.CreateAreaAsync(request);
            return Ok(area);
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpPut("UpdateArea")]
        public async Task<IActionResult> UpdateArea([FromBody] UpdateAreaRequest request)
        {
            await _areaService.UpdateAreaAsync(request);
            return Ok();
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpPut("UpdateAreaDescription")]
        public async Task<IActionResult> UpdateAreaDescription([FromBody] UpdateAreaDescriptionRequest request)
        {
            await _areaService.UpdateAreaDescriptionAsync(request);
            return Ok();
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpDelete("DeleteArea")]
        public async Task<IActionResult> DeleteArea([FromBody] DeleteAreaRequest request)
        {
            await _areaService.DeleteAreaAsync(request);
            return Ok();
        }
    }
}
