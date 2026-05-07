namespace Backend_Boarding_house_management_system.DTOs.PropertyImage.Responses
{
    using Backend_Boarding_house_management_system.DTOs.Property.Responses;

    public class PropertyImageResponse
    {
        public string Id { get; set; } = null!;
        public string PropertyId { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PropertyImageListResponse : Backend_Boarding_house_management_system.DTOs.Base.PagedResponse<PropertyImageResponse>
    {
    }

    public class PropertyImageDetailResponse : PropertyImageResponse
    {
        public PropertyResponse? Property { get; set; }
    }
}
