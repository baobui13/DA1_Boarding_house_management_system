using Backend_Boarding_house_management_system.DTOs.Base;
using Backend_Boarding_house_management_system.DTOs.User.Responses;

namespace Backend_Boarding_house_management_system.DTOs.Complaint.Responses
{
    public class ComplaintResponse
    {
        public string Id { get; set; } = null!;
        public string CreatorId { get; set; } = null!;
        public string RelatedType { get; set; } = null!;
        public string RelatedId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? AdminResponse { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ComplaintListResponse : PagedResponse<ComplaintResponse>
    {
    }

    public class ComplaintDetailResponse : ComplaintResponse
    {
        public UserResponse? Creator { get; set; }
    }
}
