using Backend_Boarding_house_management_system.DTOs.Appointment.Requests;
using Backend_Boarding_house_management_system.DTOs.Appointment.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;
using Backend_Boarding_house_management_system.Entities;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetAppointmentById")]
        public async Task<ActionResult<AppointmentResponse>> GetAppointmentById([FromQuery] GetAppointmentByIdRequest request)
        {
            var response = await _appointmentService.GetAppointmentByIdAsync(request);
            return Ok(response);
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetAppointmentsByFilter")]
        public async Task<ActionResult<AppointmentListResponse>> GetAppointmentsByFilter(
            [FromQuery] EntityFilter<Appointment> filter,
            [FromQuery] EntitySort<Appointment> sort,
            [FromQuery] EntityPage page)
        {
            var response = await _appointmentService.GetAppointmentsByFilterAsync(filter, sort, page);
            return Ok(response);
        }

        [Authorize(Roles = "Tenant,Admin")]
        [HttpPost("CreateAppointment")]
        public async Task<ActionResult<AppointmentResponse>> CreateAppointment([FromBody] CreateAppointmentRequest request)
        {
            var response = await _appointmentService.CreateAppointmentAsync(request);
            return Ok(response);
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpPut("UpdateAppointment")]
        public async Task<IActionResult> UpdateAppointment([FromBody] UpdateAppointmentRequest request)
        {
            await _appointmentService.UpdateAppointmentAsync(request);
            return Ok(new { message = "Cập nhật cuộc hẹn thành công." });
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpDelete("DeleteAppointment")]
        public async Task<IActionResult> DeleteAppointment([FromBody] DeleteAppointmentRequest request)
        {
            await _appointmentService.DeleteAppointmentAsync(request);
            return Ok(new { message = "Xóa cuộc hẹn thành công." });
        }
    }
}
