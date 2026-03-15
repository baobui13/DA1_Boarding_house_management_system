using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Amenity.Requests
{
    public class CreateAmenityRequest
    {
        [Required]
        [StringLength(50)]
        public string Id { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(255)]
        public string? Description { get; set; }
    }
}
