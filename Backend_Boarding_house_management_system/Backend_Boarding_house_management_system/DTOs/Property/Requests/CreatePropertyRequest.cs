using System.ComponentModel.DataAnnotations;
namespace Backend_Boarding_house_management_system.DTOs.Property.Requests
{
    public class CreatePropertyRequest
    {
        [Required]
        public string LandlordId { get; set; } = null!;
        public string? AreaId { get; set; }
        [Required]
        public string PropertyName { get; set; } = null!;
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        [Required]
        public decimal Size { get; set; }
        public string? Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public decimal ElectricPrice { get; set; }
        [Required]
        public decimal WaterPrice { get; set; }
        public string Status { get; set; } = "Available";
    }
}
