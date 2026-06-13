using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Boarding_house_management_system.Entities
{
    public class PropertyAspectScore
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string PropertyId { get; set; } = null!;

        [Required]
        public ReviewAspect Aspect { get; set; }

        public int PositiveCount { get; set; } = 0;
        public int NegativeCount { get; set; } = 0;
        public int NeutralCount { get; set; } = 0;
        public int TotalCount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal WeightedScore { get; set; } = 0;

        [Required]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(PropertyId))]
        public Property Property { get; set; } = null!;
    }
}
