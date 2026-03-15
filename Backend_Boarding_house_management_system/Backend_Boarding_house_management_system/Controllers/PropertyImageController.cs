using Backend_Boarding_house_management_system.DTOs.PropertyImage.Requests;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PropertyImageController : ControllerBase
    {
        private readonly IPropertyImageService _propertyImageService;
        public PropertyImageController(IPropertyImageService propertyImageService)
        {
            _propertyImageService = propertyImageService;
        }

        [HttpGet("GetPropertyImageById")]
        public async Task<IActionResult> GetPropertyImageById([FromQuery] GetPropertyImageByIdRequest request)
        {
            var image = await _propertyImageService.GetPropertyImageByIdAsync(request);
            return Ok(image);
        }

        [HttpGet("GetPropertyImagesByFilter")]
        public async Task<IActionResult> GetPropertyImagesByFilter([FromQuery] GetPropertyImagesByFilterRequest request)
        {
            var images = await _propertyImageService.GetPropertyImagesByFilterAsync(request);
            return Ok(images);
        }

        [HttpPost("CreatePropertyImage")]
        public async Task<IActionResult> CreatePropertyImage([FromForm] CreatePropertyImageRequest request)
        {
            var image = await _propertyImageService.CreatePropertyImageAsync(request);
            return Ok(image);
        }

        [HttpPut("UpdatePropertyImage")]
        public async Task<IActionResult> UpdatePropertyImage([FromBody] UpdatePropertyImageRequest request)
        {
            var result = await _propertyImageService.UpdatePropertyImageAsync(request);
            return Ok(result);
        }

        [HttpDelete("DeletePropertyImage")]
        public async Task<IActionResult> DeletePropertyImage([FromQuery] DeletePropertyImageRequest request)
        {
            var result = await _propertyImageService.DeletePropertyImageAsync(request);
            return Ok(result);
        }
    }
}
