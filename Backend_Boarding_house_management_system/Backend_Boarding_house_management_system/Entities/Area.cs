using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Boarding_house_management_system.Entities
{
    public class Area
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string Address { get; set; } = null!;

        [Column(TypeName = "decimal(10,6)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(10,6)")]
        public decimal? Longitude { get; set; }

        [Required]
        public int RoomCount { get; set; } = 0;

        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string LandlordId { get; set; } = null!;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(LandlordId))]
        public User Landlord { get; set; } = null!;

        public ICollection<Property> Properties { get; set; } = new List<Property>();
    }
}
