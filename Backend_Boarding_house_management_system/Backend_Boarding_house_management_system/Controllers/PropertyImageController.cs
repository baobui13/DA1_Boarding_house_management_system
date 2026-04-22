using Backend_Boarding_house_management_system.DTOs.PropertyImage.Requests;
using Backend_Boarding_house_management_system.DTOs.PropertyImage.Responses;
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
    public class PropertyImageController : ControllerBase
    {
        private readonly IPropertyImageService _propertyImageService;
        public PropertyImageController(IPropertyImageService propertyImageService)
        {
            _propertyImageService = propertyImageService;
        }

        [AllowAnonymous]
        [HttpGet("GetPropertyImageById")]
        public async Task<ActionResult<PropertyImageResponse>> GetPropertyImageById([FromQuery] GetPropertyImageByIdRequest request)
        {
            var image = await _propertyImageService.GetPropertyImageByIdAsync(request);
            return Ok(image);
        }

        [AllowAnonymous]
        [HttpGet("GetPropertyImageDetailById")]
        public async Task<ActionResult<PropertyImageDetailResponse>> GetPropertyImageDetailById([FromQuery] GetPropertyImageByIdRequest request)
        {
            var image = await _propertyImageService.GetPropertyImageDetailByIdAsync(request);
            return Ok(image);
        }

        [AllowAnonymous]
        [HttpGet("GetPropertyImagesByFilter")]
        public async Task<ActionResult<PropertyImageListResponse>> GetPropertyImagesByFilter(
            [FromQuery] EntityFilter<PropertyImage> filter,
            [FromQuery] EntitySort<PropertyImage> sort,
            [FromQuery] EntityPage page)
        {
            var images = await _propertyImageService.GetPropertyImagesByFilterAsync(filter, sort, page);
            return Ok(images);
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpPost("CreatePropertyImage")]
        public async Task<ActionResult<PropertyImageResponse>> CreatePropertyImage([FromForm] CreatePropertyImageRequest request)
        {
            var image = await _propertyImageService.CreatePropertyImageAsync(request);
            return Ok(image);
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpPut("UpdatePropertyImage")]
        public async Task<IActionResult> UpdatePropertyImage([FromBody] UpdatePropertyImageRequest request)
        {
            await _propertyImageService.UpdatePropertyImageAsync(request);
            return Ok();
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpDelete("DeletePropertyImage")]
        public async Task<IActionResult> DeletePropertyImage([FromQuery] DeletePropertyImageRequest request)
        {
            await _propertyImageService.DeletePropertyImageAsync(request);
            return Ok();
        }
    }
}
