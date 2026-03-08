using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Authentication.Requests
{
    public class RegisterRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, MinLength(6)]
        public string Password { get; set; } = null!;

        [Required, StringLength(100)]
        public string FullName { get; set; } = null!;

        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        [Required]
        public string Role { get; set; } = null!;
    }
}
