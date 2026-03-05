using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.User.Requests
{
    public class UpdateUserRequest
    {
        [Required]
        public string Id { get; set; } = null!;

        [StringLength(100)]
        public string? FullName { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }
    }
}