using Backend_Boarding_house_management_system.DTOs.Notification.Requests;
using Backend_Boarding_house_management_system.DTOs.Notification.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Services.Interfaces;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetNotificationById")]
        public async Task<ActionResult<NotificationResponse>> GetNotificationById([FromQuery] GetNotificationByIdRequest request)
        {
            var result = await _notificationService.GetByIdAsync(request);
            return Ok(result);
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetNotificationsByFilter")]
        public async Task<ActionResult<NotificationListResponse>> GetNotificationsByFilter(
            [FromQuery] EntityFilter<Notification> filter,
            [FromQuery] EntitySort<Notification> sort,
            [FromQuery] EntityPage page)
        {
            var result = await _notificationService.GetByFilterAsync(filter, sort, page);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("CreateNotification")]
        public async Task<ActionResult<NotificationResponse>> CreateNotification([FromBody] CreateNotificationRequest request)
        {
            var result = await _notificationService.CreateAsync(request);
            return Ok(result);
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpPut("UpdateNotification")]
        public async Task<IActionResult> UpdateNotification([FromBody] UpdateNotificationRequest request)
        {
            await _notificationService.UpdateAsync(request);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteNotification")]
        public async Task<IActionResult> DeleteNotification([FromBody] DeleteNotificationRequest request)
        {
            await _notificationService.DeleteAsync(request);
            return Ok();
        }
    }
}
