using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Complaint.Requests
{
    public class DeleteComplaintRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
