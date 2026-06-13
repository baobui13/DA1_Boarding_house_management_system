using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Boarding_house_management_system.Entities
{
    public class RatingAspect
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string RatingId { get; set; } = null!;

        [Required]
        public ReviewAspect Aspect { get; set; }

        [Required]
        public RatingAttitude Sentiment { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal? Confidence { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(RatingId))]
        public Rating Rating { get; set; } = null!;
    }
}
