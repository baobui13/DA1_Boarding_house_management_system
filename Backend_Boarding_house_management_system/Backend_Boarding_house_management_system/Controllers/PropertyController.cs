using Backend_Boarding_house_management_system.DTOs.Property.Requests;
using Backend_Boarding_house_management_system.DTOs.Property.Responses;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;
using Backend_Boarding_house_management_system.Entities;
using System.Collections.Generic;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PropertyController : ControllerBase
    {
        private readonly IPropertyService _propertyService;
        public PropertyController(IPropertyService propertyService)
        {
            _propertyService = propertyService;
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
            [FromQuery] EntityFilter<Property> filter,
            [FromQuery] EntitySort<Property> sort,
            [FromQuery] EntityPage page,
            [FromQuery] string[]? boostAspect = null)   // User điền thêm aspect khi search, ví dụ: ?boostAspect=Wifi&boostAspect=Noise
        {
            var aspectBoosts = ParseAspectBoosts(boostAspect);
            var properties = await _propertyService.GetPropertiesByFilterAsync(filter, sort, page, aspectBoosts);
            return Ok(properties);
        }

        [HttpGet("GetRecommendedProperties")]
        public async Task<ActionResult<PropertyListResponse>> GetRecommendedProperties(
            [FromQuery] EntityFilter<Property> filter,
            [FromQuery] EntitySort<Property> sort,
            [FromQuery] EntityPage page,
            [FromQuery] string[]? boostAspect = null)
        {
            // Personalized recommendations (dựa trên lịch sử cá nhân của user hiện tại)
            // + aspect user vừa chọn khi search/filter sẽ được boost mạnh
            var aspectBoosts = ParseAspectBoosts(boostAspect);
            var properties = await _propertyService.GetRecommendedPropertiesAsync(filter, sort, page, aspectBoosts);
            return Ok(properties);
        }

        [AllowAnonymous]
        [HttpGet("GetMostViewedProperties")]
        public async Task<ActionResult<PropertyListResponse>> GetMostViewedProperties(
            [FromQuery] EntityFilter<Property> filter,
            [FromQuery] EntitySort<Property> sort,
            [FromQuery] EntityPage page,
            [FromQuery] string[]? boostAspect = null)
        {
            var aspectBoosts = ParseAspectBoosts(boostAspect);
            var properties = await _propertyService.GetMostViewedPropertiesAsync(filter, sort, page, aspectBoosts);
            return Ok(properties);
        }

        [AllowAnonymous]
        [HttpGet("GetTrendingProperties")]
        public async Task<ActionResult<PropertyListResponse>> GetTrendingProperties(
            [FromQuery] EntityFilter<Property> filter,
            [FromQuery] EntitySort<Property> sort,
            [FromQuery] EntityPage page,
            [FromQuery] string[]? boostAspect = null)
        {
            var aspectBoosts = ParseAspectBoosts(boostAspect);
            var properties = await _propertyService.GetTrendingPropertiesAsync(filter, sort, page, aspectBoosts);
            return Ok(properties);
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

        [AllowAnonymous]
        [HttpGet("GetPopularPriceRanges")]
        public async Task<ActionResult<PopularPriceRangesResponse>> GetPopularPriceRanges()
        {
            var result = await _propertyService.GetPopularPriceRangesAsync();
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
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
