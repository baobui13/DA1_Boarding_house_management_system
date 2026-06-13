using Backend_Boarding_house_management_system.Entities;

namespace Backend_Boarding_house_management_system.DTOs.Property.Requests
{
    /// <summary>
    /// Đại diện cho một aspect mà user muốn ưu tiên khi search/filter.
    /// Dùng để boost các property có WeightedScore cao trên aspect này.
    /// </summary>
    public class AspectBoostRequest
    {
        public ReviewAspect Aspect { get; set; }

        /// <summary>
        /// Hệ số nhân (boost) cho aspect này. 
        /// > 1.0 nghĩa là ưu tiên mạnh hơn (ví dụ 1.8 = tăng 80% ảnh hưởng).
        /// Mặc định 1.0 nếu không chỉ định.
        /// </summary>
        public double Boost { get; set; } = 1.0;

        /// <summary>
        /// (Tùy chọn) Chỉ lấy property có WeightedScore >= giá trị này trên aspect.
        /// Nếu > 0 thì có thể dùng để lọc cứng trước khi scoring.
        /// </summary>
        public decimal? MinWeightedScore { get; set; }
    }
}
