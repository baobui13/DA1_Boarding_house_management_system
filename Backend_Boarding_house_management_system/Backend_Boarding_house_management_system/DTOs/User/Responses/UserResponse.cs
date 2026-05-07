using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.User.Responses
{
    public class UserResponse
    {
        public string Id { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Role { get; set; } = null!;

        public string Status { get; set; } = null!;

        public bool IsBlocked { get; set; }

        public string? Address { get; set; }

        public string? AvatarUrl { get; set; }

        public string? PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    public class UserListResponse : PagedResponse<UserResponse>
    {
    }
}
