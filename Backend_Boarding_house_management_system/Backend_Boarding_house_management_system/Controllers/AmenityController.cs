using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.DTOs.Amenity.Requests;
using Backend_Boarding_house_management_system.DTOs.Amenity.Responses;
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
    public class AmenityController : ControllerBase
    {
        private readonly IAmenityService _amenityService;
        public AmenityController(IAmenityService amenityService)
        {
            _amenityService = amenityService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<AmenityPagedResponse>> Get(
            [FromQuery] EntityFilter<Amenity> filter,
            [FromQuery] EntitySort<Amenity> sort,
            [FromQuery] EntityPage page)
        {
            var result = await _amenityService.GetByFilterAsync(filter, sort, page);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("all")]
        public async Task<ActionResult<List<AmenityResponse>>> GetAll()
        {
            var result = await _amenityService.GetAllAsync();
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<AmenityResponse>> GetById(string id)
        {
            var result = await _amenityService.GetByIdAsync(id);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAmenityRequest request)
        {
            await _amenityService.AddAsync(request);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateAmenityRequest request)
        {
            await _amenityService.UpdateAsync(request);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _amenityService.DeleteAsync(id);
            return Ok();
        }
    }
}
