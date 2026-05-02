using Backend_Boarding_house_management_system.DTOs.Complaint.Requests;
using Backend_Boarding_house_management_system.DTOs.Complaint.Responses;
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
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintService _complaintService;

        public ComplaintController(IComplaintService complaintService)
        {
            _complaintService = complaintService;
        }

        [HttpGet("GetComplaintById")]
        public async Task<ActionResult<ComplaintResponse>> GetComplaintById([FromQuery] GetComplaintByIdRequest request)
        {
            var complaint = await _complaintService.GetComplaintByIdAsync(request);
            return Ok(complaint);
        }

        [HttpGet("GetComplaintDetailById")]
        public async Task<ActionResult<ComplaintDetailResponse>> GetComplaintDetailById([FromQuery] GetComplaintByIdRequest request)
        {
            var complaint = await _complaintService.GetComplaintDetailByIdAsync(request);
            return Ok(complaint);
        }

        [HttpGet("GetComplaintsByFilter")]
        public async Task<ActionResult<ComplaintListResponse>> GetComplaintsByFilter(
            [FromQuery] EntityFilter<Complaint> filter,
            [FromQuery] EntitySort<Complaint> sort,
            [FromQuery] EntityPage page)
        {
            var complaints = await _complaintService.GetComplaintsByFilterAsync(filter, sort, page);
            return Ok(complaints);
        }

        [HttpPost("CreateComplaint")]
        public async Task<ActionResult<ComplaintResponse>> CreateComplaint([FromBody] CreateComplaintRequest request)
        {
            var complaint = await _complaintService.CreateComplaintAsync(request);
            return Ok(complaint);
        }

        [HttpPut("UpdateComplaint")]
        public async Task<IActionResult> UpdateComplaint([FromBody] UpdateComplaintRequest request)
        {
            await _complaintService.UpdateComplaintAsync(request);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteComplaint")]
        public async Task<IActionResult> DeleteComplaint([FromQuery] DeleteComplaintRequest request)
        {
            await _complaintService.DeleteComplaintAsync(request);
            return Ok();
        }
    }
}
