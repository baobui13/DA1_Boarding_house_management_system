using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;

namespace Backend_Boarding_house_management_system.Entities
{
    public class Property
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LandlordId { get; set; } = null!;

        [StringLength(50)]
        public string? AreaId { get; set; }

        [Required]
        [StringLength(50)]
        public string PropertyName { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Size { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public string Status { get; set; } = "Available";  // Available, Rented, Unavailable

        public string? Description { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [Column(TypeName = "decimal(10,6)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(10,6)")]
        public decimal? Longitude { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(LandlordId))]
        public User Landlord { get; set; } = null!;

        [ForeignKey(nameof(AreaId))]
        public Area? Area { get; set; }

        public ICollection<PropertyImage> PropertyImages { get; set; } = new List<PropertyImage>();
        public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<ViewHistory> ViewHistories { get; set; } = new List<ViewHistory>();
    }
}
