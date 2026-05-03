using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Property.Requests
{
    public class RejectPropertyRequest
    {
        [Required]
        public string PropertyId { get; set; } = null!;

        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string RejectionReason { get; set; } = null!;
    }
}
