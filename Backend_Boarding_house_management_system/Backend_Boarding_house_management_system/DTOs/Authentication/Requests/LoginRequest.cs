using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Authentication.Requests
{
    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
