using Backend_Boarding_house_management_system.DTOs.ViewHistory.Requests;
using Backend_Boarding_house_management_system.DTOs.ViewHistory.Responses;
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
    [Authorize(Roles = "Landlord,Tenant,Admin")]
    public class ViewHistoryController : ControllerBase
    {
        private readonly IViewHistoryService _viewHistoryService;

        public ViewHistoryController(IViewHistoryService viewHistoryService)
        {
            _viewHistoryService = viewHistoryService;
        }

        [HttpGet("GetViewHistoriesByFilter")]
        public async Task<ActionResult<ViewHistoryListResponse>> GetViewHistoriesByFilter(
            [FromQuery] EntityFilter<ViewHistory> filter,
            [FromQuery] EntitySort<ViewHistory> sort,
            [FromQuery] EntityPage page)
        {
            var result = await _viewHistoryService.GetByFilterAsync(filter, sort, page);
            return Ok(result);
        }

        [HttpPost("CreateViewHistory")]
        public async Task<ActionResult<ViewHistoryResponse>> CreateViewHistory([FromBody] CreateViewHistoryRequest request)
        {
            var result = await _viewHistoryService.CreateAsync(request);
            return Ok(result);
        }

        [HttpDelete("DeleteViewHistory")]
        public async Task<IActionResult> DeleteViewHistory([FromBody] DeleteViewHistoryRequest request)
        {
            await _viewHistoryService.DeleteAsync(request);
            return Ok();
        }
    }
}
