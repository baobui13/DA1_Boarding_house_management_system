using Backend_Boarding_house_management_system.DTOs.ViewHistory.Requests;
using Backend_Boarding_house_management_system.DTOs.ViewHistory.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ViewHistoryController : ControllerBase
    {
        [HttpGet("GetViewHistoriesByFilter")]
        public async Task<ActionResult<ViewHistoryListResponse>> GetViewHistoriesByFilter([FromQuery] GetViewHistoriesByFilterRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("CreateViewHistory")]
        public async Task<ActionResult<ViewHistoryResponse>> CreateViewHistory([FromBody] CreateViewHistoryRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("DeleteViewHistory")]
        public async Task<IActionResult> DeleteViewHistory([FromBody] DeleteViewHistoryRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
