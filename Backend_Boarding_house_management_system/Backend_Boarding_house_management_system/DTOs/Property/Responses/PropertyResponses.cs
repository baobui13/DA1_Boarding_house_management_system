namespace Backend_Boarding_house_management_system.DTOs.Property.Responses
{
    using Backend_Boarding_house_management_system.DTOs.Appointment.Responses;
    using Backend_Boarding_house_management_system.DTOs.Area.Responses;
    using Backend_Boarding_house_management_system.DTOs.Contract.Responses;
    using Backend_Boarding_house_management_system.DTOs.PropertyImage.Responses;
    using Backend_Boarding_house_management_system.DTOs.RoomAmenity.Responses;
    using Backend_Boarding_house_management_system.DTOs.User.Responses;

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

    public class PropertyDetailResponse : PropertyResponse
    {
        public UserResponse? Landlord { get; set; }
        public AreaResponse? Area { get; set; }
        public List<PropertyImageResponse> PropertyImages { get; set; } = new();
        public List<RoomAmenityResponse> RoomAmenities { get; set; } = new();
        public List<ContractResponse> Contracts { get; set; } = new();
        public List<AppointmentResponse> Appointments { get; set; } = new();
    }
}
