using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.User.Requests
{
    public class BlockUserRequest
    {
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        public bool IsBlocked { get; set; }
    }
}