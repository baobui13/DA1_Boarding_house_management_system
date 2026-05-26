using Backend_Boarding_house_management_system.DTOs.Chatbot.Requests;
using Backend_Boarding_house_management_system.DTOs.Chatbot.Responses;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        /// <summary>
        /// Gửi tin nhắn chat và nhận phản hồi AI tư vấn.
        /// Kết hợp phân tích cảm xúc (BERT+BiLSTM) và tư vấn (Gemini).
        /// </summary>
        [AllowAnonymous]
        [HttpPost("Chat")]
        public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Tin nhắn không được để trống.");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var result = await _chatbotService.ChatAsync(request, userId);
            return Ok(result);
        }

        /// <summary>
        /// Phân tích cảm xúc một đoạn văn bản (theo tinh thần bài báo BERT+BiLSTM).
        /// Trả về nhãn cảm xúc, điểm tin cậy và mức độ khẩn cấp.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("AnalyzeEmotion")]
        public async Task<ActionResult<EmotionResult>> AnalyzeEmotion([FromBody] AnalyzeEmotionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Văn bản không được để trống.");

            var result = await _chatbotService.AnalyzeEmotionAsync(request.Text);
            return Ok(result);
        }

        /// <summary>
        /// Tiếp nhận khiếu nại từ tenant, phân tích cảm xúc và đề xuất phân loại/phản hồi.
        /// </summary>
        [Authorize(Roles = "Tenant,Landlord,Admin")]
        [HttpPost("AnalyzeComplaint")]
        public async Task<ActionResult<ComplaintAnalysisResponse>> AnalyzeComplaint([FromBody] ChatComplaintRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Nội dung khiếu nại không được để trống.");

            var result = await _chatbotService.AnalyzeComplaintAsync(request);
            return Ok(result);
        }
    }
}
