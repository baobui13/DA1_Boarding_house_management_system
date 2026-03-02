using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Boarding_house_management_system.Entities
{
    public class Appointment
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string RoomId { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string UserId { get; set; } = null!;

        [Required]
        public DateTime AppointmentDateTime { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";  // Pending, Confirmed, Rejected, Cancelled

        [StringLength(255)]
        public string? Note { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(RoomId))]
        public Property Room { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}
