using Backend_Boarding_house_management_system.DTOs.Base;
using Backend_Boarding_house_management_system.DTOs.Property.Responses;
using Backend_Boarding_house_management_system.DTOs.User.Responses;

namespace Backend_Boarding_house_management_system.DTOs.Rating.Responses
{
    public class RatingResponse
    {
        public string Id { get; set; } = null!;
        public string TenantId { get; set; } = null!;
        public string PropertyId { get; set; } = null!;
        public int Stars { get; set; }
        public string Content { get; set; } = null!;
        public string AIAttitude { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class RatingListResponse : PagedResponse<RatingResponse>
    {
    }

    public class RatingDetailResponse : RatingResponse
    {
        public UserResponse? Tenant { get; set; }
        public PropertyResponse? Property { get; set; }
    }
}
