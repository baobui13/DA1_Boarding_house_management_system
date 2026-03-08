namespace Backend_Boarding_house_management_system.DTOs.Authentication.Responses
{
    public class AuthResponse
    {
        public string Token { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? AvatarUrl { get; set; }
    }
}
