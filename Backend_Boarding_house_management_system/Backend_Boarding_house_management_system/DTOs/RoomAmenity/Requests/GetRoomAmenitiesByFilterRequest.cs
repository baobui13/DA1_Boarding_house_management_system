using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.RoomAmenity.Requests
{
    public class GetRoomAmenitiesByFilterRequest : PagedRequest
    {
        public string? RoomId { get; set; }
        public string? AmenityId { get; set; }
        public string? Status { get; set; }
    }
}
