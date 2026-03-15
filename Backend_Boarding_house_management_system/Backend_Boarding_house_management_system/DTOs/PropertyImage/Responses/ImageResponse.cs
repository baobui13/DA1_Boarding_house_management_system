using System;

namespace Backend_Boarding_house_management_system.DTOs.PropertyImage.Responses
{
    public class ImageResponse
    {
        public string Id { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
