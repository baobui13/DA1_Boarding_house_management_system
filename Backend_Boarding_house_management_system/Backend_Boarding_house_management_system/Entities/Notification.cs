using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Boarding_house_management_system.Entities
{
    public class Notification
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string UserId { get; set; } = null!;

        [Required]
        public string Type { get; set; } = null!;  // Invoice, Appointment, Contract, Message, System

        [Required]
        [StringLength(255)]
        public string Content { get; set; } = null!;

        [Required]
        public bool IsRead { get; set; } = false;

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? RelatedId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}
