using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.User.Requests
{
    public class UpdateUserAvatarRequest
    {
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string AvatarUrl { get; set; } = null!;
    }
}