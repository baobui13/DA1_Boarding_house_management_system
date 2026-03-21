using Backend_Boarding_house_management_system.DTOs.Notification.Requests;
using Backend_Boarding_house_management_system.DTOs.Notification.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetNotificationById")]
        public async Task<ActionResult<NotificationResponse>> GetNotificationById([FromQuery] GetNotificationByIdRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetNotificationsByFilter")]
        public async Task<ActionResult<NotificationListResponse>> GetNotificationsByFilter([FromQuery] GetNotificationsByFilterRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("CreateNotification")]
        public async Task<ActionResult<NotificationResponse>> CreateNotification([FromBody] CreateNotificationRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateNotification")]
        public async Task<IActionResult> UpdateNotification([FromBody] UpdateNotificationRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteNotification")]
        public async Task<IActionResult> DeleteNotification([FromBody] DeleteNotificationRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
