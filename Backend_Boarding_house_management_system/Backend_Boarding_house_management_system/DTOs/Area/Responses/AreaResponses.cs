namespace Backend_Boarding_house_management_system.DTOs.Area.Responses
{
    using Backend_Boarding_house_management_system.DTOs.Property.Responses;
    using Backend_Boarding_house_management_system.DTOs.User.Responses;

    public class AreaResponse
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int RoomCount { get; set; }
        public string? Description { get; set; }
        public string LandlordId { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class AreaListResponse : Backend_Boarding_house_management_system.DTOs.Base.PagedResponse<AreaResponse>
    {
    }

    public class AreaDetailResponse : AreaResponse
    {
        public UserResponse? Landlord { get; set; }
        public List<PropertyResponse> Properties { get; set; } = new();
    }
}
