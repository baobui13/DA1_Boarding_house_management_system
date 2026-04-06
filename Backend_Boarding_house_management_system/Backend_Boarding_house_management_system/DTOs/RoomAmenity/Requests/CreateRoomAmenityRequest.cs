using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.RoomAmenity.Requests
{
    public class CreateRoomAmenityRequest
    {
        [Required]
        public string PropertyId { get; set; } = null!;

        [Required]
        public string AmenityId { get; set; } = null!;

        public string Status { get; set; } = "Working";

        public string? Note { get; set; }
    }
}
