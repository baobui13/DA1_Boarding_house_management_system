using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Authentication.Requests
{
    public class GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; } = null!;
    }
}
