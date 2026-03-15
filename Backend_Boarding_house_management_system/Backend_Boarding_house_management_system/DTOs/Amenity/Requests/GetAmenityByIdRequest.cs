using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Amenity.Requests
{
    public class GetAmenityByIdRequest
    {
        [Required]
        [StringLength(50)]
        public string Id { get; set; } = null!;
    }
}
