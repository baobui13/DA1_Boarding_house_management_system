namespace Backend_Boarding_house_management_system.DTOs.Property.Responses
{
    public class PropertyResponse
    {
        public string Id { get; set; } = null!;
        public string LandlordId { get; set; } = null!;
        public string? AreaId { get; set; }
        public string PropertyName { get; set; } = null!;
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal Size { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal ElectricPrice { get; set; }
        public decimal WaterPrice { get; set; }
        public string Status { get; set; } = null!;
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PropertyListResponse : Backend_Boarding_house_management_system.DTOs.Base.PagedResponse<PropertyResponse>
    {
    }
}
