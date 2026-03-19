using Backend_Boarding_house_management_system.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Backend_Boarding_house_management_system.Extensions
{
    public static class DatabaseExtensions
    {
        // 1. Extension để đăng ký Database vào Service Container
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            return services;
        }

        // 2. Extension để kiểm tra kết nối Database, bắt lỗi chi tiết và tự động chạy Migration
        public static async Task<WebApplication> CheckDatabaseConnectionAsync(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("DatabaseConnectionCheck");

                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                try
                {
                    logger.LogInformation("I [INFO] Attempting to connect to the database and check migrations...");

                    // MigrateAsync sẽ TỰ ĐỘNG kết nối. Nếu sai Pass/Port, nó sẽ ném ra lỗi chi tiết rớt xuống catch.
                    // Nếu kết nối thành công, nó sẽ tự động tạo bảng luôn. Gộp 2 việc làm 1!
                    await dbContext.Database.MigrateAsync();

                    logger.LogInformation("O [SUCCESS] Successfully connected to PostgreSQL Database (Supabase) and migrations applied.");
                }
                catch (Exception ex)
                {
                    // In ra chính xác lỗi kết nối hoặc lỗi migration
                    logger.LogError("X [ERROR] Database connection or migration FAILED! Exact error: {Message}", ex.Message);

                    if (ex.InnerException != null)
                    {
                        logger.LogError("--> Inner Error Details: {InnerMessage}", ex.InnerException.Message);
                    }
                }
            }

            return app;
        }
    }
}