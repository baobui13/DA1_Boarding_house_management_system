using Backend_Boarding_house_management_system.DTOs.Appointment.Requests;
using Backend_Boarding_house_management_system.DTOs.Appointment.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetAppointmentById")]
        public async Task<ActionResult<AppointmentResponse>> GetAppointmentById([FromQuery] GetAppointmentByIdRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetAppointmentsByFilter")]
        public async Task<ActionResult<AppointmentListResponse>> GetAppointmentsByFilter([FromQuery] GetAppointmentsByFilterRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Tenant,Admin")]
        [HttpPost("CreateAppointment")]
        public async Task<ActionResult<AppointmentResponse>> CreateAppointment([FromBody] CreateAppointmentRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpPut("UpdateAppointment")]
        public async Task<IActionResult> UpdateAppointment([FromBody] UpdateAppointmentRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpDelete("DeleteAppointment")]
        public async Task<IActionResult> DeleteAppointment([FromBody] DeleteAppointmentRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
