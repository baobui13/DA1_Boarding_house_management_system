using CloudinaryDotNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Backend_Boarding_house_management_system.Extensions
{
    public static class CloudinaryExtensions
    {
        public static async Task<WebApplication> CheckCloudinaryConnectionAsync(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("CloudinaryConnectionCheck");

                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                try
                {
                    var cloudName = config["CloudinarySettings:CloudName"];
                    var apiKey = config["CloudinarySettings:ApiKey"];
                    var apiSecret = config["CloudinarySettings:ApiSecret"];

                    if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
                    {
                        logger.LogWarning("I [WARNING] Missing Cloudinary configuration in appsettings.json!");
                        return app; // Thoát sớm nếu thiếu cấu hình
                    }

                    // Sử dụng HttpClient để gọi trực tiếp Admin API Ping của Cloudinary
                    var pingUrl = $"https://api.cloudinary.com/v1_1/{cloudName}/ping";
                    using var httpClient = new HttpClient();

                    // Cloudinary Admin API yêu cầu xác thực Basic Auth bằng ApiKey và ApiSecret
                    var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{apiKey}:{apiSecret}"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                    // Gọi Ping
                    var response = await httpClient.GetAsync(pingUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        logger.LogInformation("O [SUCCESS] Successfully accessed Cloudinary (CloudName: {CloudName})", cloudName);
                    }
                    else
                    {
                        // Đọc lỗi chi tiết từ Cloudinary nếu có (thường do sai API Key/Secret)
                        var errorContent = await response.Content.ReadAsStringAsync();
                        logger.LogWarning("I [WARNING] Cloudinary connection rejected. StatusCode: {StatusCode}, Details: {Error}", response.StatusCode, errorContent);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "X [ERROR] Failed to send request to Cloudinary. Please check your network connection.");
                }
            }

            return app;
        }
    }
}
