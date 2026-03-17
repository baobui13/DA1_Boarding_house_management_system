using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Notification.Requests
{
    public class GetNotificationByIdRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
