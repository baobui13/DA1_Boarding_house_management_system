using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.Notification.Responses
{
    public class NotificationResponse
    {
        public string Id { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Content { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime Timestamp { get; set; }
        public string? RelatedId { get; set; }
    }

    public class NotificationListResponse : PagedResponse<NotificationResponse>
    {
    }
}
