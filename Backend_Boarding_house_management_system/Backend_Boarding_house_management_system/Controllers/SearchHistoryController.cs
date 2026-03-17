using Backend_Boarding_house_management_system.DTOs.SearchHistory.Requests;
using Backend_Boarding_house_management_system.DTOs.SearchHistory.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchHistoryController : ControllerBase
    {
        [HttpGet("GetSearchHistoriesByFilter")]
        public async Task<ActionResult<SearchHistoryListResponse>> GetSearchHistoriesByFilter([FromQuery] GetSearchHistoriesByFilterRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("CreateSearchHistory")]
        public async Task<ActionResult<SearchHistoryResponse>> CreateSearchHistory([FromBody] CreateSearchHistoryRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("DeleteSearchHistory")]
        public async Task<IActionResult> DeleteSearchHistory([FromBody] DeleteSearchHistoryRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
