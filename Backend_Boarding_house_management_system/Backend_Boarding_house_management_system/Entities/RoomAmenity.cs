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
        public string RoomId { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string AmenityId { get; set; } = null!;

        [Required]
        public string Status { get; set; } = "Working";  // Working, Broken, Repairing

        [StringLength(255)]
        public string? Note { get; set; }

        [ForeignKey(nameof(RoomId))]
        public Property Room { get; set; } = null!;

        [ForeignKey(nameof(AmenityId))]
        public Amenity Amenity { get; set; } = null!;
    }
}
