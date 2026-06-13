using System.Collections.Generic;

namespace Backend_Boarding_house_management_system.Options
{
    /// <summary>
    /// Cấu hình cho hệ thống Recommendation (Scoring Engine).
    /// Hỗ trợ nhiều Profile/Mode để tùy biến sâu cách xếp hạng property.
    /// </summary>
    public class RecommendationOptions
    {
        public const string SectionName = "Recommendation";

        /// <summary>
        /// Mode mặc định khi không chỉ định.
        /// </summary>
        public RecommendationMode DefaultMode { get; set; } = RecommendationMode.PersonalMatch;

        /// <summary>
        /// Trọng số cơ sở dùng chung (có thể override theo profile).
        /// </summary>
        public ScoringWeights BaseWeights { get; set; } = new ScoringWeights();

        /// <summary>
        /// Định nghĩa các profile cụ thể. Key là tên mode (string).
        /// Nếu không có thì dùng BaseWeights + logic mặc định của mode.
        /// </summary>
        public Dictionary<string, ScoringWeights> Profiles { get; set; } = new Dictionary<string, ScoringWeights>(System.StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Các trọng số dùng để tính điểm property trong recommendation.
    /// Giá trị càng cao thì yếu tố đó càng ảnh hưởng mạnh đến thứ tự.
    /// </summary>
    public class ScoringWeights
    {
        // === History signals (area, price, amenity) ===
        public double PersonalHistory { get; set; } = 1.0;
        public double GlobalHistory { get; set; } = 0.55;

        // === Aspect signals (từ PropertyAspectScore + user preference) ===
        public double AspectMatch { get; set; } = 1.6;           // Trọng số tổng cho phần aspect
        public double AspectGlobalQuality { get; set; } = 0.65;  // Hệ số nhân với avg WeightedScore

        // === Price & Area & Amenity cụ thể ===
        public double PriceFit { get; set; } = 28;
        public double AreaMatch { get; set; } = 35;
        public double AmenityMatchPerItem { get; set; } = 6;
        public double MaxAmenityBonus { get; set; } = 22;

        // === Penalty & special rules ===
        public double NegativeAspectPenaltyMultiplier { get; set; } = 1.2;  // Dùng cho AvoidNegatives
        public double LowEvidencePenalty { get; set; } = 0;                 // Trừ điểm nếu TotalCount quá thấp (tùy profile)

        // === Base score ===
        public double BaseScore { get; set; } = 10;
    }

    /// <summary>
    /// Các chế độ (profile) recommendation có sẵn.
    /// Có thể mở rộng thêm sau.
    /// </summary>
    public enum RecommendationMode
    {
        /// <summary>
        /// Mặc định: ưu tiên khớp sở thích aspect cá nhân + chất lượng aspect tổng thể.
        /// Phù hợp cho người dùng đã có lịch sử rating.
        /// </summary>
        PersonalMatch = 0,

        /// <summary>
        /// Tập trung mạnh vào chất lượng aspect toàn cục (avg WeightedScore cao + bằng chứng mạnh).
        /// Giảm bớt ảnh hưởng của lịch sử cá nhân.
        /// </summary>
        HighAspectQuality = 1,

        /// <summary>
        /// Cân bằng giữa lịch sử cá nhân, chất lượng aspect, và tín hiệu phổ biến.
        /// </summary>
        Balanced = 2,

        /// <summary>
        /// Nhạy cảm với giá: tăng mạnh ảnh hưởng của price fit.
        /// </summary>
        PriceSensitive = 3,

        /// <summary>
        /// Khuyến khích khám phá: giảm personal bias, tăng global quality + tín hiệu đa dạng.
        /// Phù hợp cho người dùng mới hoặc muốn xem thêm lựa chọn.
        /// </summary>
        Explore = 4,

        /// <summary>
        /// Tránh các khía cạnh tiêu cực mà user từng chê (negative ratings).
        /// Áp dụng penalty mạnh nếu property yếu ở aspect user đã rate Negative.
        /// </summary>
        AvoidNegatives = 5
    }
}
