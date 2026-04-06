using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Boarding_house_management_system.Entities
{
    public class RoomAmenity
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string PropertyId { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string AmenityId { get; set; } = null!;

        [Required]
        public string Status { get; set; } = "Working";  // Working, Broken, Repairing

        [StringLength(255)]
        public string? Note { get; set; }

        [ForeignKey(nameof(PropertyId))]
        public Property Property { get; set; } = null!;

        [ForeignKey(nameof(AmenityId))]
        public Amenity Amenity { get; set; } = null!;
    }
}
