using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Message.Requests
{
    public class UpdateMessageRequest
    {
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        public bool IsRead { get; set; }
    }
}
