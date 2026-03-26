using Backend_Boarding_house_management_system.DTOs.SearchHistory.Requests;
using Backend_Boarding_house_management_system.DTOs.SearchHistory.Responses;
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
    public class SearchHistoryController : ControllerBase
    {
        private readonly ISearchHistoryService _searchHistoryService;

        public SearchHistoryController(ISearchHistoryService searchHistoryService)
        {
            _searchHistoryService = searchHistoryService;
        }

        [HttpGet("GetSearchHistoriesByFilter")]
        public async Task<ActionResult<SearchHistoryListResponse>> GetSearchHistoriesByFilter(
            [FromQuery] EntityFilter<SearchHistory> filter,
            [FromQuery] EntitySort<SearchHistory> sort,
            [FromQuery] EntityPage page)
        {
            var result = await _searchHistoryService.GetByFilterAsync(filter, sort, page);
            return Ok(result);
        }

        [HttpPost("CreateSearchHistory")]
        public async Task<ActionResult<SearchHistoryResponse>> CreateSearchHistory([FromBody] CreateSearchHistoryRequest request)
        {
            var result = await _searchHistoryService.CreateAsync(request);
            return Ok(result);
        }

        [HttpDelete("DeleteSearchHistory")]
        public async Task<IActionResult> DeleteSearchHistory([FromBody] DeleteSearchHistoryRequest request)
        {
            await _searchHistoryService.DeleteAsync(request);
            return Ok();
        }
    }
}
