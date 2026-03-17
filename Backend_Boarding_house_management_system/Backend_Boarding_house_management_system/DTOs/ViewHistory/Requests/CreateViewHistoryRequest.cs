using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.ViewHistory.Requests
{
    public class CreateViewHistoryRequest
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public string RoomId { get; set; } = null!;
    }
}
