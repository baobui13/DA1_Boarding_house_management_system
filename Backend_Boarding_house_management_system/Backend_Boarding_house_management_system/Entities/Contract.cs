using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Boarding_house_management_system.Entities
{
    public class Contract
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string PropertyId { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string TenantId { get; set; } = null!;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Deposit { get; set; }

        public string? Terms { get; set; }

        [StringLength(255)]
        public string? ContractFileUrl { get; set; }

        [Required]
        public string Status { get; set; } = "Active";  // Active, Expired, Terminated

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(PropertyId))]
        public Property Property { get; set; } = null!;

        [ForeignKey(nameof(TenantId))]
        public User Tenant { get; set; } = null!;


        // Trong class Contract
        public DateOnly? ActualEndDate { get; set; }

        public string? HandoverNote { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DeductionAmount { get; set; } = 0m;

        public string? DeductionReason { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; } = 0m;

        public string? HandoverConfirmedBy { get; set; }

        public DateTime? HandoverConfirmedAt { get; set; }



        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
