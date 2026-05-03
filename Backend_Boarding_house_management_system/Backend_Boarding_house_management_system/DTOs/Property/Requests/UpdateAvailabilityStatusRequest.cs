using System.ComponentModel.DataAnnotations;
using Backend_Boarding_house_management_system.Entities;

namespace Backend_Boarding_house_management_system.DTOs.Property.Requests
{
    public class UpdateAvailabilityStatusRequest
    {
        [Required]
        public string PropertyId { get; set; } = null!;

        [Required]
        public AvailabilityStatusEnum AvailabilityStatus { get; set; }
    }
}
