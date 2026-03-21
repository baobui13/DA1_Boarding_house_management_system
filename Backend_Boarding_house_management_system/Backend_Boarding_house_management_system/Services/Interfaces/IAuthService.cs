using Backend_Boarding_house_management_system.DTOs.Authentication.Requests;
using Backend_Boarding_house_management_system.DTOs.Authentication.Responses;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest dto);
        Task<AuthResponse> LoginAsync(LoginRequest dto);
        Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest dto);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest dto);
        Task<bool> LogoutAsync(string userId);
    }
}
