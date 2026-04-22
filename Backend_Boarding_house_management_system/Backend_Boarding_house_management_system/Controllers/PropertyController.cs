using Backend_Boarding_house_management_system.DTOs.Property.Requests;
using Backend_Boarding_house_management_system.DTOs.Property.Responses;
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
            [FromQuery] EntityPage page)
        {
            var properties = await _propertyService.GetPropertiesByFilterAsync(filter, sort, page);
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

        [Authorize(Roles = "Landlord,Admin")]
        [HttpDelete("DeleteProperty")]
        public async Task<IActionResult> DeleteProperty([FromBody] DeletePropertyRequest request)
        {
            await _propertyService.DeletePropertyAsync(request);
            return Ok();
        }
    }
}
