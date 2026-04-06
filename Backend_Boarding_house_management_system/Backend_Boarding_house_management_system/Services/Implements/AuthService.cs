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
            }
            catch
            {
                throw new UnauthorizedException("Token Google khong hop le.");
            }

            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                user = new User
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    FullName = payload.Name,
                    AvatarUrl = payload.Picture,
                    Role = "Tenant",
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

                var roleResult = await _userManager.AddToRoleAsync(user, "Tenant");
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new ValidationException($"Gan vai tro cho tai khoan Google that bai: {errors}");
                }
            }

            var token = await GenerateJwtTokenAsync(user);
            var refreshToken = GenerateRefreshToken();

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
            await _userManager.GetRolesAsync(user);

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

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:DurationInMinutes"] ?? "30")),
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
