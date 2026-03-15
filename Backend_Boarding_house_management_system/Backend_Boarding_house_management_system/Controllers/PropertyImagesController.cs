using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Backend_Boarding_house_management_system.DTOs.PropertyImage.Requests;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/properties/images")]
    public class PropertyImagesController : ControllerBase
    {
        private readonly IPropertyImageService _propertyImageService;

        public PropertyImagesController(IPropertyImageService propertyImageService)
        {
            _propertyImageService = propertyImageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] ImageUploadRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("File không hợp lệ.");

            var result = await _propertyImageService.UploadAndSaveImageAsync(request);

            if (result == null)
                return BadRequest("Có lỗi xảy ra khi upload ảnh.");

            // Trả về HTTP 201 Created cùng với data
            return CreatedAtAction(nameof(UploadImage), new { id = result.Id }, result);
        }
    }
}
