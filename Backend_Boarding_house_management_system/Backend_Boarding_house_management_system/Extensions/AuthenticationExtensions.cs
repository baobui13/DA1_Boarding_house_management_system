using Backend_Boarding_house_management_system.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Backend_Boarding_house_management_system.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Setup Identity
            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<Data.AppDbContext>()
            .AddDefaultTokenProviders();

            // 2. Setup Authentication (JWT & Google)
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
                };
            })
            .AddGoogle(options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
            });

            // 3. Setup Authorization Policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("LandlordOnly", policy => policy.RequireRole("Landlord"));
                options.AddPolicy("TenantOnly", policy => policy.RequireRole("Tenant"));
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            });

            return services;
        }

        // Đổi thành Extension cho WebApplication để dễ gọi ở Program.cs
        public static void CheckAuthenticationConnections(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AuthConnectionCheck");
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // 1. KIỂM TRA JWT
            var jwtKey = configuration["Jwt:Key"];
            var jwtIssuer = configuration["Jwt:Issuer"];
            var jwtAudience = configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
            {
                logger.LogWarning("I [WARNING] Missing JWT configuration in appsettings.json.");
            }
            else
            {
                try
                {
                    // Test tạo và validate token
                    var key = Encoding.UTF8.GetBytes(jwtKey);
                    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new System.Security.Claims.ClaimsIdentity([new System.Security.Claims.Claim("test", "value")]),
                        Expires = DateTime.UtcNow.AddMinutes(1),
                        Issuer = jwtIssuer,
                        Audience = jwtAudience,
                        SigningCredentials = new SigningCredentials(
                            new SymmetricSecurityKey(key),
                            SecurityAlgorithms.HmacSha256Signature)
                    };

                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var tokenString = tokenHandler.WriteToken(token);

                    tokenHandler.ValidateToken(tokenString, new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = false, // Chỉ test key, bỏ qua test thời gian
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    }, out _);

                    logger.LogInformation("O [SUCCESS] JWT configuration is valid and Token Generator is working properly.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "X [ERROR] Failed to initialize JWT Token. Please check the length of Jwt:Key (should be >= 16 characters).");
                }
            }

            // 2. KIỂM TRA GOOGLE (Chỉ cần check config tồn tại)
            var googleClientId = configuration["Authentication:Google:ClientId"];
            var googleClientSecret = configuration["Authentication:Google:ClientSecret"];

            if (string.IsNullOrEmpty(googleClientId) || string.IsNullOrEmpty(googleClientSecret))
            {
                logger.LogWarning("I [WARNING] Missing Google Auth configuration in appsettings.json.");
            }
            else
            {
                logger.LogInformation("O [SUCCESS] Google Auth ClientId configuration detected.");
            }
        }
    }
}