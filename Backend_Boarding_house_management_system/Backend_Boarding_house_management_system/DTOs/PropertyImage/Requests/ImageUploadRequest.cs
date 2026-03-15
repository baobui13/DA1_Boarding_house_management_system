using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.PropertyImage.Requests
{
    public class ImageUploadRequest
    {
        [Required]
        public string PropertyId { get; set; } = null!;

        public bool IsPrimary { get; set; } = false;

        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
