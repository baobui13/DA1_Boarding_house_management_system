using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace Backend_Boarding_house_management_system.Entities
{
    public class User
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = null!;

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [StringLength(255)]
        public string? PasswordHash { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [Required]
        public string Role { get; set; } = null!;  // "Admin", "Landlord", "Tenant"

        [Required]
        public bool IsVerified { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(255)]
        public string? AvatarUrl { get; set; }

        // Navigation properties
        public ICollection<Area> Areas { get; set; } = new List<Area>();
        public ICollection<Property> Properties { get; set; } = new List<Property>();
        public ICollection<Contract> ContractsAsTenant { get; set; } = new List<Contract>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Message> MessagesSent { get; set; } = new List<Message>();
        public ICollection<Message> MessagesReceived { get; set; } = new List<Message>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();
        public ICollection<ViewHistory> ViewHistories { get; set; } = new List<ViewHistory>();
        public ICollection<TenantDocument> TenantDocuments { get; set; } = new List<TenantDocument>();
    }
}
