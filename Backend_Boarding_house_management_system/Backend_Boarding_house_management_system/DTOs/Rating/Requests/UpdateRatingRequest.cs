using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Rating.Requests
{
    public class UpdateRatingRequest
    {
        [Required]
        public string Id { get; set; } = null!;

        [Range(1, 5)]
        public int? Stars { get; set; }

        [StringLength(2000)]
        public string? Content { get; set; }

        [StringLength(50)]
        public string? AIAttitude { get; set; }
    }
}
