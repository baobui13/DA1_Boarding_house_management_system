using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.SearchHistory.Requests
{
    public class GetSearchHistoriesByFilterRequest : PagedRequest
    {
        public string? UserId { get; set; }
        public DateTime? TimestampFrom { get; set; }
        public DateTime? TimestampTo { get; set; }
    }
}
