using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.Message.Responses
{
    public class MessageResponse
    {
        public string Id { get; set; } = null!;
        public string SenderId { get; set; } = null!;
        public string ReceiverId { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
        public string? RoomId { get; set; }
        public string? ContractId { get; set; }
    }

    public class MessageListResponse : PagedResponse<MessageResponse>
    {
    }
}
