using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Boarding_house_management_system.Entities
{
    public class Message
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string SenderId { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string ReceiverId { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsRead { get; set; } = false;

        [StringLength(50)]
        public string? PropertyId { get; set; }

        [StringLength(50)]
        public string? ContractId { get; set; }

        [ForeignKey(nameof(SenderId))]
        public User Sender { get; set; } = null!;

        [ForeignKey(nameof(ReceiverId))]
        public User Receiver { get; set; } = null!;

        [ForeignKey(nameof(PropertyId))]
        public Property? Property { get; set; }

        [ForeignKey(nameof(ContractId))]
        public Contract? Contract { get; set; }
    }
}
