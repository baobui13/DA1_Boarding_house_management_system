using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Notification.Requests
{
    public class CreateNotificationRequest
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public string Type { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string Content { get; set; } = null!;

        [StringLength(50)]
        public string? RelatedId { get; set; }
    }
}
