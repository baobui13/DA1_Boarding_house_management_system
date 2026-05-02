using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Complaint.Requests
{
    public class UpdateComplaintRequest
    {
        [Required]
        public string Id { get; set; } = null!;

        [StringLength(50)]
        public string? RelatedType { get; set; }

        public string? RelatedId { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(4000)]
        public string? Content { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        [StringLength(4000)]
        public string? AdminResponse { get; set; }

        public DateTime? ResolvedAt { get; set; }
    }
}
