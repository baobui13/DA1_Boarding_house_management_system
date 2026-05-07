using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Property.Requests
{
    public class ApprovePropertyRequest
    {
        [Required]
        public string PropertyId { get; set; } = null!;
    }
}
