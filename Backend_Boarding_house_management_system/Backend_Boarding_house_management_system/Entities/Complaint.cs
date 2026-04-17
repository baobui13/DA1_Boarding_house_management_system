using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Boarding_house_management_system.Entities
{
    public class Complaint
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string CreatorId { get; set; } = null!;

        [Required]
        public string RelatedType { get; set; } = null!; // Invoice, Contract, Property

        [Required]
        [StringLength(50)]
        public string RelatedId { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        public string Status { get; set; } = "Pending"; // Pending, Processing, Resolved

        public string? AdminResponse { get; set; }

        public DateTime? ResolvedAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(CreatorId))]
        public User Creator { get; set; } = null!;
    }
}
