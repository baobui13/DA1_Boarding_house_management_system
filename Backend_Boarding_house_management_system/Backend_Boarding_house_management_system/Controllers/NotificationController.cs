using Backend_Boarding_house_management_system.DTOs.Notification.Requests;
using Backend_Boarding_house_management_system.DTOs.Notification.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        [HttpGet("GetNotificationById")]
        public async Task<ActionResult<NotificationResponse>> GetNotificationById([FromQuery] GetNotificationByIdRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpGet("GetNotificationsByFilter")]
        public async Task<ActionResult<NotificationListResponse>> GetNotificationsByFilter([FromQuery] GetNotificationsByFilterRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("CreateNotification")]
        public async Task<ActionResult<NotificationResponse>> CreateNotification([FromBody] CreateNotificationRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPut("UpdateNotification")]
        public async Task<IActionResult> UpdateNotification([FromBody] UpdateNotificationRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("DeleteNotification")]
        public async Task<IActionResult> DeleteNotification([FromBody] DeleteNotificationRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
