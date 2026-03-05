using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.User.Requests
{
    public class GetUserByIdOrEmailRequest
    {
        [Required]
        public string? Id { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }
}