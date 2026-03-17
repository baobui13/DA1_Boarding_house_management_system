using Backend_Boarding_house_management_system.DTOs.PropertyImage.Requests;
using Backend_Boarding_house_management_system.DTOs.PropertyImage.Responses;
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
        public async Task<ActionResult<PropertyImageResponse>> GetPropertyImageById([FromQuery] GetPropertyImageByIdRequest request)
        {
            var image = await _propertyImageService.GetPropertyImageByIdAsync(request);
            return Ok(image);
        }

        [HttpGet("GetPropertyImagesByFilter")]
        public async Task<ActionResult<PropertyImageListResponse>> GetPropertyImagesByFilter([FromQuery] GetPropertyImagesByFilterRequest request)
        {
            var images = await _propertyImageService.GetPropertyImagesByFilterAsync(request);
            return Ok(images);
        }

        [HttpPost("CreatePropertyImage")]
        public async Task<ActionResult<PropertyImageResponse>> CreatePropertyImage([FromForm] CreatePropertyImageRequest request)
        {
            var image = await _propertyImageService.CreatePropertyImageAsync(request);
            return Ok(image);
        }

        [HttpPut("UpdatePropertyImage")]
        public async Task<IActionResult> UpdatePropertyImage([FromBody] UpdatePropertyImageRequest request)
        {
            await _propertyImageService.UpdatePropertyImageAsync(request);
            return Ok();
        }

        [HttpDelete("DeletePropertyImage")]
        public async Task<IActionResult> DeletePropertyImage([FromQuery] DeletePropertyImageRequest request)
        {
            await _propertyImageService.DeletePropertyImageAsync(request);
            return Ok();
        }
    }
}
