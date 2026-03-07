using System.ComponentModel.DataAnnotations;
namespace Backend_Boarding_house_management_system.DTOs.Area.Requests
{
    public class UpdateAreaRequest
    {
        [Required]
        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? RoomCount { get; set; }
        public string? Description { get; set; }
    }
}
