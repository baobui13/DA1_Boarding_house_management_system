using Backend_Boarding_house_management_system.DTOs.Property.Requests;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PropertyController : ControllerBase
    {
        private readonly IPropertyService _propertyService;
        public PropertyController(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        [HttpGet("GetPropertyById")]
        public async Task<IActionResult> GetPropertyById([FromQuery] GetPropertyByIdRequest request)
        {
            var property = await _propertyService.GetPropertyByIdAsync(request);
            return Ok(property);
        }

        [HttpGet("GetPropertiesByFilter")]
        public async Task<IActionResult> GetPropertiesByFilter([FromQuery] GetPropertiesByFilterRequest request)
        {
            var properties = await _propertyService.GetPropertiesByFilterAsync(request);
            return Ok(properties);
        }

        [HttpPost("CreateProperty")]
        public async Task<IActionResult> CreateProperty([FromBody] CreatePropertyRequest request)
        {
            var property = await _propertyService.CreatePropertyAsync(request);
            return Ok(property);
        }

        [HttpPut("UpdateProperty")]
        public async Task<IActionResult> UpdateProperty([FromBody] UpdatePropertyRequest request)
        {
            await _propertyService.UpdatePropertyAsync(request);
            return NoContent();
        }

        [HttpDelete("DeleteProperty")]
        public async Task<IActionResult> DeleteProperty([FromBody] DeletePropertyRequest request)
        {
            await _propertyService.DeletePropertyAsync(request);
            return NoContent();
        }
    }
}
