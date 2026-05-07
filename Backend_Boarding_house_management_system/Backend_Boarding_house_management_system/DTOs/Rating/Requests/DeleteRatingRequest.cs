using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Rating.Requests
{
    public class DeleteRatingRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
