using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Boarding_house_management_system.Entities
{
    public class ViewHistory
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string UserId { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string RoomId { get; set; } = null!;

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [ForeignKey(nameof(RoomId))]
        public Property Room { get; set; } = null!;
    }
}
