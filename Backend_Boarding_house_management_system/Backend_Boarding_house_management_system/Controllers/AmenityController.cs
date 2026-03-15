using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.DTOs.Amenity.Requests;
using Backend_Boarding_house_management_system.DTOs.Amenity.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AmenityController : ControllerBase
    {
        private readonly IAmenityService _amenityService;
        public AmenityController(IAmenityService amenityService)
        {
            _amenityService = amenityService;
        }

        [HttpGet]
        public async Task<ActionResult<AmenityPagedResponse>> Get([FromQuery] GetAmenitiesByFilterRequest request)
        {
            var result = await _amenityService.GetByFilterAsync(request);
            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<AmenityResponse>>> GetAll()
        {
            var result = await _amenityService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AmenityResponse>> GetById(string id)
        {
            var result = await _amenityService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAmenityRequest request)
        {
            await _amenityService.AddAsync(request);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateAmenityRequest request)
        {
            await _amenityService.UpdateAsync(request);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _amenityService.DeleteAsync(id);
            return Ok();
        }
    }
}
