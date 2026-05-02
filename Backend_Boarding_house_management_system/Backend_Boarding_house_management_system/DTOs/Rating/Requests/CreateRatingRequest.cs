using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Rating.Requests
{
    public class CreateRatingRequest
    {
        [Required]
        public string TenantId { get; set; } = null!;

        [Required]
        public string PropertyId { get; set; } = null!;

        [Range(1, 5)]
        public int Stars { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string AIAttitude { get; set; } = null!;
    }
}
