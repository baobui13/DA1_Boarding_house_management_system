namespace Backend_Boarding_house_management_system.DTOs.Chatbot.Requests
{
    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public List<ChatHistoryItem> History { get; set; } = new();
        public string UserRole { get; set; } = "tenant"; // "tenant" | "landlord"
    }

    public class ChatHistoryItem
    {
        public string Role { get; set; } = string.Empty; // "user" | "model"
        public string Text { get; set; } = string.Empty;
    }

    public class AnalyzeEmotionRequest
    {
        public string Text { get; set; } = string.Empty;
    }

    public class ChatComplaintRequest
    {
        public string Content { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string TenantName { get; set; } = "Ẩn danh";
    }
}
