using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Options;
using System.Collections.Generic;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    /// <summary>
    /// Interface cho Scoring Engine của hệ thống đề xuất.
    /// Cho phép tách biệt hoàn toàn logic tính điểm khỏi PropertyService,
    /// hỗ trợ nhiều RecommendationMode / Profile khác nhau để tùy biến sâu.
    /// </summary>
    public interface IPropertyScorer
    {
        /// <summary>
        /// Tính điểm relevance cho một property theo mode được chọn.
        /// Hỗ trợ thêm aspect boosts từ search hiện tại (user điền aspect khi search).
        /// </summary>
        /// <param name="property">Property (đã include PropertyAspectScores...)</param>
        /// <param name="preference">UserPreference từ lịch sử (có thể null)</param>
        /// <param name="userPositiveAspects">Aspect user thích từ lịch sử rating</param>
        /// <param name="userNegativeAspects">Aspect user chê từ lịch sử rating</param>
        /// <param name="mode">Chế độ recommendation</param>
        /// <param name="searchAspectBoosts">
        /// Aspect mà user đang ưu tiên trong lần search/filter này (từ UI).
        /// Key = Aspect, Value = boost factor (ví dụ 1.5 nghĩa là tăng 50% ảnh hưởng của aspect đó).
        /// Property có WeightedScore cao trên những aspect này sẽ được đẩy lên mạnh.
        /// </param>
        /// <param name="userId">User hiện tại</param>
        /// <returns>Điểm số càng cao càng xếp trước</returns>
        double CalculateScore(
            Property property,
            UserPreference? preference,
            HashSet<ReviewAspect>? userPositiveAspects,
            HashSet<ReviewAspect>? userNegativeAspects,
            RecommendationMode mode,
            IDictionary<ReviewAspect, double>? searchAspectBoosts = null,
            string? userId = null);
    }

    /// <summary>
    /// Preference đơn giản của user (giữ nguyên từ logic cũ để tương thích).
    /// Sau này có thể thay bằng UserAspectProfile phong phú hơn.
    /// </summary>
    public record UserPreference(
        HashSet<string> AreaIds,
        decimal? PriceMean,
        decimal? PriceMin,
        decimal? PriceMax,
        HashSet<string> AmenityIds,
        HashSet<ReviewAspect> AspectInterests);
}
