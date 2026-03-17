using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Message.Requests
{
    public class CreateMessageRequest
    {
        [Required]
        public string SenderId { get; set; } = null!;

        [Required]
        public string ReceiverId { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        public string? RoomId { get; set; }

        public string? ContractId { get; set; }
    }
}
