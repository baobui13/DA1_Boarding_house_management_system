using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Message.Requests
{
    public class DeleteMessageRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
