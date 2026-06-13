using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.PropertyImage.Requests
{
    public class ReplacePropertyImageRequest
    {
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
