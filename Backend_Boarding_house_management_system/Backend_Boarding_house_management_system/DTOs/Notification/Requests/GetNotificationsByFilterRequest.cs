using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.Notification.Requests
{
    public class GetNotificationsByFilterRequest : PagedRequest
    {
        public string? UserId { get; set; }
        public string? Type { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? TimestampFrom { get; set; }
        public DateTime? TimestampTo { get; set; }
    }
}
