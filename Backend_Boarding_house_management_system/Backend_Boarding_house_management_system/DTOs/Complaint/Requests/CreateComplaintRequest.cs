using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Complaint.Requests
{
    public class CreateComplaintRequest
    {
        [Required]
        public string CreatorId { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string RelatedType { get; set; } = null!;

        [Required]
        public string RelatedId { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(4000)]
        public string Content { get; set; } = null!;
    }
}
