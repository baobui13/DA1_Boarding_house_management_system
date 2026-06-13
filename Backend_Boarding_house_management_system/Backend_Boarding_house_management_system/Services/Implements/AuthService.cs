using AutoMapper;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.DTOs.Authentication.Requests;
using Backend_Boarding_house_management_system.DTOs.Authentication.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            AppDbContext context,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _mapper = mapper;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new ConflictException($"Email '{dto.Email}' da ton tai.");
            }

            var allowedRoles = new[] { "Admin", "Landlord", "Tenant" };
            if (!allowedRoles.Contains(dto.Role))
            {
                throw new ValidationException("Role khong hop le. Chi chap nhan Admin, Landlord hoac Tenant.");
            }

            var user = _mapper.Map<User>(dto);
            user.UserName = dto.Email;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ValidationException($"Dang ky that bai: {errors}");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new ValidationException($"Gan vai tro that bai: {errors}");
            }

            return _mapper.Map<AuthResponse>(user);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                throw new UnauthorizedException("Sai thong tin dang nhap.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                throw new UnauthorizedException("Sai thong tin dang nhap.");
            }

            var token = await GenerateJwtTokenAsync(user);
            var refreshToken = GenerateRefreshToken();

            // Revoke previous active refresh tokens for this user (hygiene)
            var previousTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
                .ToListAsync();
            foreach (var t in previousTokens)
            {
                t.IsRevoked = true;
            }

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiryTime = DateTime.UtcNow.AddDays(30),
                UserId = user.Id
            });
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(30),
                Email = user.Email!,
                FullName = user.FullName,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl
            };
        }

        public async Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest dto)
        {
            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { _configuration["Authentication:Google:ClientId"]! }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);

                // Bắt buộc email phải được Google xác minh (rất quan trọng cho bảo mật)
                if (!payload.EmailVerified)
                {
                    throw new UnauthorizedException("Email tu Google chua duoc xac minh.");
                }
            }
            catch (UnauthorizedException)
            {
                throw;
            }
            catch
            {
                throw new UnauthorizedException("Token Google khong hop le.");
            }

            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                const string defaultRole = "Tenant";
                user = new User
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    FullName = payload.Name,
                    AvatarUrl = payload.Picture,
                    Role = defaultRole,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new ValidationException($"Khong the tao tai khoan qua Google: {errors}");
                }

                var roleResult = await _userManager.AddToRoleAsync(user, defaultRole);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new ValidationException($"Gan vai tro cho tai khoan Google that bai: {errors}");
                }
            }

            var token = await GenerateJwtTokenAsync(user);
            var refreshToken = GenerateRefreshToken();

            // Revoke previous active refresh tokens for this user (hygiene)
            var previousTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
                .ToListAsync();
            foreach (var t in previousTokens)
            {
                t.IsRevoked = true;
            }

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiryTime = DateTime.UtcNow.AddDays(30),
                UserId = user.Id
            });
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(30),
                Email = user.Email!,
                FullName = user.FullName,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl
            };
        }

        public async Task<AuthResponse> FacebookLoginAsync(FacebookLoginRequest dto)
        {
            var appId = _configuration["Authentication:Facebook:AppId"];
            var appSecret = _configuration["Authentication:Facebook:AppSecret"];

            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
            {
                throw new UnauthorizedException("Cau hinh Facebook chua duoc thiet lap tren server.");
            }

            string? email = null;
            string? name = null;
            string? picture = null;

            try
            {
                using var httpClient = new HttpClient();

                // 1. Validate token with debug_token endpoint (ensures token belongs to our app)
                var debugUrl = $"https://graph.facebook.com/debug_token?input_token={dto.AccessToken}&access_token={appId}|{appSecret}";
                var debugResp = await httpClient.GetAsync(debugUrl);
                if (!debugResp.IsSuccessStatusCode)
                {
                    throw new UnauthorizedException("Token Facebook khong hop le.");
                }

                var debugContent = await debugResp.Content.ReadAsStringAsync();
                using var debugDoc = JsonDocument.Parse(debugContent);
                var data = debugDoc.RootElement.GetProperty("data");
                var isValid = data.GetProperty("is_valid").GetBoolean();
                var appIdInToken = data.GetProperty("app_id").GetString();

                if (!isValid || appIdInToken != appId)
                {
                    throw new UnauthorizedException("Token Facebook khong hop le hoac khong thuoc ung dung.");
                }

                // 2. Fetch profile: id, name, email, picture
                var meUrl = $"https://graph.facebook.com/me?fields=id,name,email,picture.type(large)&access_token={dto.AccessToken}";
                var meResp = await httpClient.GetAsync(meUrl);
                if (!meResp.IsSuccessStatusCode)
                {
                    throw new UnauthorizedException("Khong the lay thong tin nguoi dung tu Facebook.");
                }

                var meContent = await meResp.Content.ReadAsStringAsync();
                using var meDoc = JsonDocument.Parse(meContent);

                if (meDoc.RootElement.TryGetProperty("email", out var emailProp))
                {
                    email = emailProp.GetString();
                }
                name = meDoc.RootElement.GetProperty("name").GetString();

                if (meDoc.RootElement.TryGetProperty("picture", out var picProp) &&
                    picProp.TryGetProperty("data", out var dataProp) &&
                    dataProp.TryGetProperty("url", out var urlProp))
                {
                    picture = urlProp.GetString();
                }

                if (string.IsNullOrEmpty(email))
                {
                    throw new UnauthorizedException("Tai khoan Facebook khong cung cap email. Hay cap quyen email hoac su dung phuong thuc dang nhap khac.");
                }
            }
            catch (Exception ex) when (!(ex is UnauthorizedException || ex is ValidationException))
            {
                throw new UnauthorizedException("Xac thuc Facebook that bai.");
            }

            var user = await _userManager.FindByEmailAsync(email!);

            if (user == null)
            {
                const string defaultRole = "Tenant";
                user = new User
                {
                    UserName = email,
                    Email = email,
                    FullName = name ?? "Facebook User",
                    AvatarUrl = picture,
                    Role = defaultRole,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new ValidationException($"Khong the tao tai khoan qua Facebook: {errors}");
                }

                var roleResult = await _userManager.AddToRoleAsync(user, defaultRole);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new ValidationException($"Gan vai tro cho tai khoan Facebook that bai: {errors}");
                }
            }

            var token = await GenerateJwtTokenAsync(user);
            var refreshToken = GenerateRefreshToken();

            // Revoke previous active refresh tokens for this user (hygiene)
            var previousTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
                .ToListAsync();
            foreach (var t in previousTokens)
            {
                t.IsRevoked = true;
            }

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiryTime = DateTime.UtcNow.AddDays(30),
                UserId = user.Id
            });
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(30),
                Email = user.Email!,
                FullName = user.FullName,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest dto)
        {
            ClaimsPrincipal principal;
            try
            {
                principal = GetPrincipalFromExpiredToken(dto.Token);
            }
            catch (SecurityTokenException)
            {
                throw new UnauthorizedException("Token khong hop le.");
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
            {
                throw new NotFoundException("Nguoi dung khong ton tai.");
            }

            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken && rt.UserId == user.Id);

            if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedException("Refresh token khong hop le hoac da het han.");
            }

            refreshToken.IsRevoked = true;

            var newJwtToken = await GenerateJwtTokenAsync(user);
            var newRefreshToken = GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                ExpiryTime = DateTime.UtcNow.AddDays(30),
                UserId = user.Id
            });

            await _context.SaveChangesAsync();

            var response = _mapper.Map<AuthResponse>(user);
            response.Token = newJwtToken;
            response.RefreshToken = newRefreshToken;
            response.RefreshTokenExpiration = DateTime.UtcNow.AddDays(30);

            return response;
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException($"Khong tim thay nguoi dung voi Id '{userId}'.");
            }

            var refreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in refreshTokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("AvatarUrl", user.AvatarUrl ?? "")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Sử dụng ExpireHours từ config (thay vì DurationInMinutes bị ignore trước đây)
            var expireHours = double.TryParse(_configuration["Jwt:ExpireHours"], out var h) ? h : 2;
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expireHours),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}
