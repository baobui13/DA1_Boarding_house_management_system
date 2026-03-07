using System.ComponentModel.DataAnnotations;
namespace Backend_Boarding_house_management_system.DTOs.Property.Requests
{
    public class UpdatePropertyRequest
    {
        [Required]
        public string Id { get; set; } = null!;
        public string? PropertyName { get; set; }
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Size { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Status { get; set; }
        public string? AreaId { get; set; }
        public string? RejectionReason { get; set; }
    }
}
