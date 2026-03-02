using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Boarding_house_management_system.Entities
{
    public class TenantDocument
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string TenantId { get; set; } = null!;

        [Required]
        public string DocumentType { get; set; } = null!;  // IDCard, ResidencePermit, Other

        [Required]
        [StringLength(255)]
        public string FileUrl { get; set; } = null!;

        [StringLength(255)]
        public string? Note { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(TenantId))]
        public User Tenant { get; set; } = null!;
    }
}
