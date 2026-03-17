using Backend_Boarding_house_management_system.DTOs.Appointment.Requests;
using Backend_Boarding_house_management_system.DTOs.Appointment.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        [HttpGet("GetAppointmentById")]
        public async Task<ActionResult<AppointmentResponse>> GetAppointmentById([FromQuery] GetAppointmentByIdRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpGet("GetAppointmentsByFilter")]
        public async Task<ActionResult<AppointmentListResponse>> GetAppointmentsByFilter([FromQuery] GetAppointmentsByFilterRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("CreateAppointment")]
        public async Task<ActionResult<AppointmentResponse>> CreateAppointment([FromBody] CreateAppointmentRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPut("UpdateAppointment")]
        public async Task<IActionResult> UpdateAppointment([FromBody] UpdateAppointmentRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("DeleteAppointment")]
        public async Task<IActionResult> DeleteAppointment([FromBody] DeleteAppointmentRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
