using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.RoomAmenity.Responses
{
    public class RoomAmenityResponse
    {
        public string Id { get; set; } = null!;
        public string PropertyId { get; set; } = null!;
        public string AmenityId { get; set; } = null!;
        public string AmenityName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? Note { get; set; }
    }

    public class RoomAmenityListResponse : PagedResponse<RoomAmenityResponse>
    {
    }
}
