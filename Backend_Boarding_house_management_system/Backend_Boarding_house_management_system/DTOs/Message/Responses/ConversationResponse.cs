namespace Backend_Boarding_house_management_system.DTOs.Message.Responses
{
    public class ConversationResponse
    {
        public string ContactId { get; set; } = null!;
        public string ContactName { get; set; } = null!;
        public string? ContactAvatarUrl { get; set; }
        public string ContactRole { get; set; } = null!;
        public MessageResponse LastMessage { get; set; } = null!;
        public int UnreadCount { get; set; }
    }
}
