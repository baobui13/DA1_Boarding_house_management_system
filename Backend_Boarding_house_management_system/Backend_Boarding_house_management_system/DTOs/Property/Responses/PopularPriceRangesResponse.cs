using System.Collections.Generic;

namespace Backend_Boarding_house_management_system.DTOs.Property.Responses
{
    /// <summary>
    /// Một khoảng giá phổ biến cùng số lượng property hiện đang match.
    /// </summary>
    public class PopularPriceRange
    {
        public string Label { get; set; } = null!;
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Kết quả cho GetPopularPriceRanges — danh sách các khoảng giá phổ biến hiện nay.
    /// </summary>
    public class PopularPriceRangesResponse
    {
        public List<PopularPriceRange> Ranges { get; set; } = new List<PopularPriceRange>();
        public int TotalPropertiesConsidered { get; set; }
    }
}