using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.RoomAmenity.Requests
{
    public class DeleteRoomAmenityRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
