using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.RoomAmenity.Requests
{
    public class UpdateRoomAmenityRequest
    {
        [Required]
        public string Id { get; set; } = null!;

        public string? Status { get; set; }

        public string? Note { get; set; }
    }
}
