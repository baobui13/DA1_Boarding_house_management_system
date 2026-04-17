using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.ViewHistory.Requests
{
    public class GetViewHistoriesByFilterRequest : PagedRequest
    {
        public string? UserId { get; set; }
        public string? PropertyId { get; set; }
        public DateTime? TimestampFrom { get; set; }
        public DateTime? TimestampTo { get; set; }
    }
}
