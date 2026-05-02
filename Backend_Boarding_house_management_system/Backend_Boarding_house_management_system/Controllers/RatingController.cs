using Backend_Boarding_house_management_system.DTOs.Rating.Requests;
using Backend_Boarding_house_management_system.DTOs.Rating.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plainquire.Filter;
using Plainquire.Page;
using Plainquire.Sort;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RatingController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        [AllowAnonymous]
        [HttpGet("GetRatingById")]
        public async Task<ActionResult<RatingResponse>> GetRatingById([FromQuery] GetRatingByIdRequest request)
        {
            var rating = await _ratingService.GetRatingByIdAsync(request);
            return Ok(rating);
        }

        [AllowAnonymous]
        [HttpGet("GetRatingDetailById")]
        public async Task<ActionResult<RatingDetailResponse>> GetRatingDetailById([FromQuery] GetRatingByIdRequest request)
        {
            var rating = await _ratingService.GetRatingDetailByIdAsync(request);
            return Ok(rating);
        }

        [AllowAnonymous]
        [HttpGet("GetRatingsByFilter")]
        public async Task<ActionResult<RatingListResponse>> GetRatingsByFilter(
            [FromQuery] EntityFilter<Rating> filter,
            [FromQuery] EntitySort<Rating> sort,
            [FromQuery] EntityPage page)
        {
            var ratings = await _ratingService.GetRatingsByFilterAsync(filter, sort, page);
            return Ok(ratings);
        }

        [Authorize(Roles = "Tenant,Admin")]
        [HttpPost("CreateRating")]
        public async Task<ActionResult<RatingResponse>> CreateRating([FromBody] CreateRatingRequest request)
        {
            var rating = await _ratingService.CreateRatingAsync(request);
            return Ok(rating);
        }

        [Authorize(Roles = "Tenant,Admin")]
        [HttpPut("UpdateRating")]
        public async Task<IActionResult> UpdateRating([FromBody] UpdateRatingRequest request)
        {
            await _ratingService.UpdateRatingAsync(request);
            return Ok();
        }

        [Authorize(Roles = "Tenant,Admin")]
        [HttpDelete("DeleteRating")]
        public async Task<IActionResult> DeleteRating([FromQuery] DeleteRatingRequest request)
        {
            await _ratingService.DeleteRatingAsync(request);
            return Ok();
        }
    }
}
