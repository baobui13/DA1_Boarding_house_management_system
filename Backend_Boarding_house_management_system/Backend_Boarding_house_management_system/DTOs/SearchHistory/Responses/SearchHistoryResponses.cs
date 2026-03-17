using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.SearchHistory.Responses
{
    public class SearchHistoryResponse
    {
        public string Id { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string Filters { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }

    public class SearchHistoryListResponse : PagedResponse<SearchHistoryResponse>
    {
    }
}
