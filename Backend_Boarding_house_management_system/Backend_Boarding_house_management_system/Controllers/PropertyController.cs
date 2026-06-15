using Backend_Boarding_house_management_system.DTOs.Property.Requests;
using Backend_Boarding_house_management_system.DTOs.Property.Responses;
using Backend_Boarding_house_management_system.DTOs.SearchHistory.Requests;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Options;
using System.Collections.Generic;
using System.Text.Json;
using System.Security.Claims;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PropertyController : ControllerBase
    {
        private readonly IPropertyService _propertyService;
        private readonly ISearchHistoryService _searchHistoryService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PropertyController(
            IPropertyService propertyService,
            ISearchHistoryService searchHistoryService,
            IHttpContextAccessor httpContextAccessor)
        {
            _propertyService = propertyService;
            _searchHistoryService = searchHistoryService;
            _httpContextAccessor = httpContextAccessor;
        }

        [AllowAnonymous]
        [HttpGet("GetPropertyById")]
        public async Task<ActionResult<PropertyResponse>> GetPropertyById([FromQuery] GetPropertyByIdRequest request)
        {
            var property = await _propertyService.GetPropertyByIdAsync(request);
            return Ok(property);
        }

        [AllowAnonymous]
        [HttpGet("GetPropertyDetailById")]
        public async Task<ActionResult<PropertyDetailResponse>> GetPropertyDetailById([FromQuery] GetPropertyByIdRequest request)
        {
            var property = await _propertyService.GetPropertyDetailByIdAsync(request);
            return Ok(property);
        }

        [AllowAnonymous]
        [HttpGet("GetPropertiesByFilter")]
        public async Task<ActionResult<PropertyListResponse>> GetPropertiesByFilter(
            [FromQuery] string? landlordId,
            [FromQuery] EntityFilter<Property> filter,
            [FromQuery] EntitySort<Property> sort,
            [FromQuery] EntityPage page,
            [FromQuery] string[]? boostAspect = null,
            [FromQuery] string? recommendationMode = null)
        {
            if (!string.IsNullOrEmpty(landlordId)) filter.Add(x => x.LandlordId, "==" + landlordId);
            var aspectBoosts = ParseAspectBoosts(boostAspect);
            var mode = ParseRecommendationMode(recommendationMode);
            await LogAspectBoostSearchIfAuthenticatedAsync(boostAspect);
            var properties = await _propertyService.GetPropertiesByFilterAsync(filter, sort, page, aspectBoosts, mode);
            return Ok(properties);
        }

        [HttpGet("GetRecommendedProperties")]
        public async Task<ActionResult<PropertyListResponse>> GetRecommendedProperties(
            [FromQuery] EntityFilter<Property> filter,
            [FromQuery] EntitySort<Property> sort,
            [FromQuery] EntityPage page,
            [FromQuery] string[]? boostAspect = null,
            [FromQuery] string? recommendationMode = null)
        {
            // Personalized recommendations (dựa trên lịch sử cá nhân của user hiện tại)
            // + aspect user vừa chọn khi search/filter sẽ được boost mạnh
            var aspectBoosts = ParseAspectBoosts(boostAspect);
            var mode = ParseRecommendationMode(recommendationMode);
            await LogAspectBoostSearchIfAuthenticatedAsync(boostAspect);
            var properties = await _propertyService.GetRecommendedPropertiesAsync(filter, sort, page, aspectBoosts, mode);
            return Ok(properties);
        }

        [AllowAnonymous]
        [HttpGet("GetMostViewedProperties")]
        public async Task<ActionResult<PropertyListResponse>> GetMostViewedProperties(
            [FromQuery] EntityFilter<Property> filter,
            [FromQuery] EntitySort<Property> sort,
            [FromQuery] EntityPage page,
            [FromQuery] string[]? boostAspect = null,
            [FromQuery] string? recommendationMode = null)
        {
            var aspectBoosts = ParseAspectBoosts(boostAspect);
            var mode = ParseRecommendationMode(recommendationMode);
            await LogAspectBoostSearchIfAuthenticatedAsync(boostAspect);
            var properties = await _propertyService.GetMostViewedPropertiesAsync(filter, sort, page, aspectBoosts, mode);
            return Ok(properties);
        }

        [AllowAnonymous]
        [HttpGet("GetTrendingProperties")]
        public async Task<ActionResult<PropertyListResponse>> GetTrendingProperties(
            [FromQuery] EntityFilter<Property> filter,
            [FromQuery] EntitySort<Property> sort,
            [FromQuery] EntityPage page,
            [FromQuery] string[]? boostAspect = null,
            [FromQuery] string? recommendationMode = null)
        {
            var aspectBoosts = ParseAspectBoosts(boostAspect);
            var mode = ParseRecommendationMode(recommendationMode);
            await LogAspectBoostSearchIfAuthenticatedAsync(boostAspect);
            var properties = await _propertyService.GetTrendingPropertiesAsync(filter, sort, page, aspectBoosts, mode);
            return Ok(properties);
        }

        /// <summary>
        /// Parse query param recommendationMode từ client thành enum.
        /// Chấp nhận tên mode hoặc số (0=PersonalMatch, 1=HighAspectQuality,...).
        /// Mặc định PersonalMatch nếu không truyền hoặc sai.
        /// </summary>
        private static RecommendationMode ParseRecommendationMode(string? modeStr)
        {
            if (string.IsNullOrWhiteSpace(modeStr))
                return RecommendationMode.PersonalMatch;

            if (Enum.TryParse<RecommendationMode>(modeStr, ignoreCase: true, out var mode))
                return mode;

            return RecommendationMode.PersonalMatch;
        }

        /// <summary>
        /// Parse query param boostAspect=Wifi&boostAspect=Landlord thành dictionary để scorer dùng.
        /// Mặc định boost factor = 1.6 (có thể sau nâng cấp cho phép chỉ định weight).
        /// </summary>
        private static IDictionary<ReviewAspect, double>? ParseAspectBoosts(string[]? boostAspects)
        {
            if (boostAspects == null || boostAspects.Length == 0)
                return null;

            var result = new Dictionary<ReviewAspect, double>();
            const double defaultBoost = 1.6;

            foreach (var aspStr in boostAspects)
            {
                if (string.IsNullOrWhiteSpace(aspStr)) continue;
                if (Enum.TryParse<ReviewAspect>(aspStr, ignoreCase: true, out var aspect))
                {
                    if (!result.ContainsKey(aspect))
                        result[aspect] = defaultBoost;
                }
            }

            return result.Count > 0 ? result : null;
        }

        /// <summary>
        /// Tự động ghi nhận vào SearchHistory khi user thực hiện search có boostAspect.
        /// Điều này đảm bảo lịch sử search chứa thông tin aspect boost để BuildUserPreferenceAsync
        /// và recommendation sau này có thể tận dụng (kết hợp với aspect từ rating).
        /// </summary>
        private async Task LogAspectBoostSearchIfAuthenticatedAsync(string[]? boostAspect)
        {
            if (boostAspect == null || boostAspect.Length == 0)
                return;

            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return;

            try
            {
                var boostList = new List<string>();
                var aspectBoostsObj = new Dictionary<string, double>();

                foreach (var asp in boostAspect)
                {
                    if (!string.IsNullOrWhiteSpace(asp))
                    {
                        boostList.Add(asp);
                        aspectBoostsObj[asp] = 1.6;
                    }
                }

                if (boostList.Count == 0)
                    return;

                var filtersJson = JsonSerializer.Serialize(new
                {
                    boostAspects = boostList,
                    aspectBoosts = aspectBoostsObj,
                    source = "propertySearchWithBoost",
                    timestamp = DateTime.UtcNow
                });

                var request = new CreateSearchHistoryRequest
                {
                    UserId = userId,
                    Filters = filtersJson
                };

                await _searchHistoryService.CreateAsync(request);
            }
            catch
            {
                // Không để lỗi ghi search history làm fail request list property
            }
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpGet("GetMyProperties")]
        public async Task<ActionResult<PropertyListResponse>> GetMyProperties(
            [FromQuery] EntitySort<Property> sort,
            [FromQuery] EntityPage page)
        {
            var properties = await _propertyService.GetMyPropertiesAsync(sort, page);
            return Ok(properties);
        }

        [AllowAnonymous]
        [HttpGet("GetPopularPriceRanges")]
        public async Task<ActionResult<PopularPriceRangesResponse>> GetPopularPriceRanges()
        {
            var result = await _propertyService.GetPopularPriceRangesAsync();
            return Ok(result);
        }

        // [Authorize(Roles = "Admin")]
        [AllowAnonymous]
        [HttpGet("GetModerationProperties")]
        public async Task<ActionResult<PropertyListResponse>> GetModerationProperties([FromQuery] GetModerationPropertiesRequest request)
        {
            var properties = await _propertyService.GetModerationPropertiesAsync(request);
            return Ok(properties);
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpPost("CreateProperty")]
        public async Task<ActionResult<PropertyResponse>> CreateProperty([FromBody] CreatePropertyRequest request)
        {
            var property = await _propertyService.CreatePropertyAsync(request);
            return Ok(property);
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpPut("UpdateProperty")]
        public async Task<IActionResult> UpdateProperty([FromBody] UpdatePropertyRequest request)
        {
            await _propertyService.UpdatePropertyAsync(request);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("ApproveProperty")]
        public async Task<IActionResult> ApproveProperty([FromBody] ApprovePropertyRequest request)
        {
            await _propertyService.ApprovePropertyAsync(request);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("RejectProperty")]
        public async Task<IActionResult> RejectProperty([FromBody] RejectPropertyRequest request)
        {
            await _propertyService.RejectPropertyAsync(request);
            return Ok();
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpPut("UpdateAvailabilityStatus")]
        public async Task<IActionResult> UpdateAvailabilityStatus([FromBody] UpdateAvailabilityStatusRequest request)
        {
            await _propertyService.UpdateAvailabilityStatusAsync(request);
            return Ok();
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpDelete("DeleteProperty")]
        public async Task<IActionResult> DeleteProperty([FromBody] DeletePropertyRequest request)
        {
            await _propertyService.DeletePropertyAsync(request);
            return Ok();
        }
    }
}
