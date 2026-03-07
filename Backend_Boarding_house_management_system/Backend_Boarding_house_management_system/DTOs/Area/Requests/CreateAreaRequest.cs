using System.ComponentModel.DataAnnotations;
namespace Backend_Boarding_house_management_system.DTOs.Area.Requests
{
    public class CreateAreaRequest
    {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string Address { get; set; } = null!;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int RoomCount { get; set; } = 0;
        public string? Description { get; set; }
        [Required]
        public string LandlordId { get; set; } = null!;
    }
}
