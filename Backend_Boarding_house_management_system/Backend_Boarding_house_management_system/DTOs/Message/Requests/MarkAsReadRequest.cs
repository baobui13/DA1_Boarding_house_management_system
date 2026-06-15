using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Message.Requests
{
    public class MarkAsReadRequest
    {
        [Required]
        public string SenderId { get; set; } = null!;
    }
}
