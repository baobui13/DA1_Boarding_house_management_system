using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Boarding_house_management_system.Entities
{
    public class PropertyImage
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string PropertyId { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string ImageUrl { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string PublicId { get; set; } = null!;

        [Required]
        public bool IsPrimary { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(PropertyId))]
        public Property Property { get; set; } = null!;
    }
}
