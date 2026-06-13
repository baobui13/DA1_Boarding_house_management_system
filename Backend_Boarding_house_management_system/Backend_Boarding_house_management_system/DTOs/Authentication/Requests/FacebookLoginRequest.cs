using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Authentication.Requests
{
    public class FacebookLoginRequest
    {
        [Required]
        public string AccessToken { get; set; } = null!;
    }
}
