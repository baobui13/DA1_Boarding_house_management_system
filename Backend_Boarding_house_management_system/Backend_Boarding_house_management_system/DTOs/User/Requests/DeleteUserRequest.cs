using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.User.Requests
{
    public class DeleteUserRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}