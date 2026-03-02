using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Boarding_house_management_system.Entities
{
    public class Invoice
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string ContractId { get; set; } = null!;

        [Required]
        public DateTime Period { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RentAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ElectricityUsage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ElectricityCost { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? WaterUsage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? WaterCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OtherFees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Penalty { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [StringLength(255)]
        public string? Note { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";  // Pending, Partial, Paid

        [StringLength(255)]
        public string? InvoiceUrl { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(ContractId))]
        public Contract Contract { get; set; } = null!;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
