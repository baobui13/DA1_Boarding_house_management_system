using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Authentication.Requests
{
    public class RefreshTokenRequest
    {
        [Required]
        public string Token { get; set; } = null!;

        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}
