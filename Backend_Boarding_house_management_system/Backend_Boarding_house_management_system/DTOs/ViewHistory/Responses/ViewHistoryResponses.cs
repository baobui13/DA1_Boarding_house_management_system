using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.ViewHistory.Responses
{
    public class ViewHistoryResponse
    {
        public string Id { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string RoomId { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }

    public class ViewHistoryListResponse : PagedResponse<ViewHistoryResponse>
    {
    }
}
