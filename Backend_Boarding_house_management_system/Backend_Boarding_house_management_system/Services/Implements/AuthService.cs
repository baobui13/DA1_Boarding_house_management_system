using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend_Boarding_house_management_system.DTOs.Authentication.Requests;
using Backend_Boarding_house_management_system.DTOs.Authentication.Responses;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest dto)
        {
            // Kiểm tra Email tồn tại đã được Identity xử lý qua cấu hình RequireUniqueEmail
            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                // EmailConfirmed = false (Mặc định theo Diagram)
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Đăng ký thất bại: {errors}");
            }

            // Gán Role (Lưu ý: Bạn cần đảm bảo các Role đã được Seed trong DB trước đó)
            await _userManager.AddToRoleAsync(user, dto.Role);

            // TODO: Bắn Event/Message gửi Email xác thực tại đây

            return new AuthResponse
            {
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
                // Token null vì theo flow cần phải đăng nhập lại hoặc click link xác nhận
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) throw new Exception("Lỗi: Sai thông tin đăng nhập.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (!result.Succeeded) throw new Exception("Lỗi: Sai thông tin đăng nhập.");

            var token = await GenerateJwtTokenAsync(user);

            return new AuthResponse
            {
                Token = token,
                Email = user.Email!,
                FullName = user.FullName,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl
            };
        }

        public async Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest dto)
        {
            // Xác thực Token từ Google
            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { _configuration["Authentication:Google:ClientId"]! }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);
            }
            catch
            {
                throw new Exception("Lỗi: Token Google không hợp lệ.");
            }

            // Kiểm tra user đã tồn tại chưa
            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                // Nếu chưa có, tạo user mới (Đăng ký qua OAuth)
                user = new User
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    FullName = payload.Name,
                    AvatarUrl = payload.Picture,
                    Role = "Tenant", // Mặc định gán Tenant khi login qua Google, có thể cho update sau
                    EmailConfirmed = true, // OAuth đã verify email
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);
                if (createResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Tenant");
                }
                else
                {
                    throw new Exception("Lỗi: Không thể tạo tài khoản qua Google.");
                }
            }

            var token = await GenerateJwtTokenAsync(user);

            return new AuthResponse
            {
                Token = token,
                Email = user.Email!,
                FullName = user.FullName,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl
            };
        }

        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role), // Lưu trữ Role như trong Flow Sync Avatar/Role
                new Claim("AvatarUrl", user.AvatarUrl ?? "")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7), // Token sống 7 ngày
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
