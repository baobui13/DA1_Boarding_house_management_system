using Backend_Boarding_house_management_system.Entities;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    /// <summary>
    /// ABSA (Aspect-Based Sentiment Analysis) cho hệ thống rating.
    /// Dùng để phân tích Content review của tenant thành các cặp (Aspect, Sentiment).
    /// Hỗ trợ recommendation property theo sở thích aspect của user.
    /// </summary>
    public interface IAspectAnalysisService
    {
        /// <summary>
        /// Phân tích review text + stars thành danh sách aspect sentiments.
        /// Triển khai thực tế sẽ gọi model AI (LLM có function calling / structured output,
        /// hoặc dedicated ABSA model).
        /// </summary>
        Task<List<AnalyzedAspect>> AnalyzeReviewAspectsAsync(string content, int stars);
    }

    /// <summary>
    /// Kết quả một aspect sentiment từ AI analysis.
    /// </summary>
    public record AnalyzedAspect(ReviewAspect Aspect, RatingAttitude Sentiment, decimal? Confidence = null);
}
