using Backend_Boarding_house_management_system.DTOs.Chatbot.Requests;
using Backend_Boarding_house_management_system.DTOs.Chatbot.Responses;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IChatbotService
    {
        /// <summary>
        /// Gửi tin nhắn tới AI và nhận phản hồi tư vấn.
        /// </summary>
        Task<ChatResponse> ChatAsync(ChatRequest request, string? userId = null);

        /// <summary>
        /// Phân tích cảm xúc một đoạn văn bản.
        /// </summary>
        Task<EmotionResult> AnalyzeEmotionAsync(string text);

        /// <summary>
        /// Tiếp nhận khiếu nại, phân tích cảm xúc và đề xuất phân loại.
        /// </summary>
        Task<ComplaintAnalysisResponse> AnalyzeComplaintAsync(ChatComplaintRequest request);
    }
}
