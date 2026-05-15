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

                    await EnsureCriticalSchemaAsync(dbContext);
                    await EnsureMigrationHistoryConsistencyAsync(dbContext);

                    // MigrateAsync sẽ TỰ ĐỘNG kết nối. Nếu sai Pass/Port, nó sẽ ném ra lỗi chi tiết rớt xuống catch.
                    // Nếu kết nối thành công, nó sẽ tự động tạo bảng luôn. Gộp 2 việc làm 1!
                    await dbContext.Database.MigrateAsync();

                    var forceReseedPropertyData = string.Equals(
                        Environment.GetEnvironmentVariable("QLT_FORCE_RESEED_PROPERTY_DATA"),
                        "true",
                        StringComparison.OrdinalIgnoreCase
                    );

                    if (forceReseedPropertyData)
                    {
                        logger.LogInformation("I [INFO] Force reseeding property catalog for development data...");
                        await Backend_Boarding_house_management_system.Data.DatabaseSeeder.ReseedPropertyCatalogAsync(dbContext);
                    }
                    else
                    {
                        await Backend_Boarding_house_management_system.Data.DatabaseSeeder.SeedDataAsync(dbContext);
                    }

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

        private static async Task EnsureCriticalSchemaAsync(AppDbContext dbContext)
        {
            await dbContext.Database.ExecuteSqlRawAsync("""
                ALTER TABLE "Users"
                ADD COLUMN IF NOT EXISTS "CCCD" character varying(20) NOT NULL DEFAULT '';
            """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                ALTER TABLE "Users"
                ADD COLUMN IF NOT EXISTS "ReputationScore" integer NOT NULL DEFAULT 0;
            """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                ALTER TABLE "Properties"
                ADD COLUMN IF NOT EXISTS "ElectricPrice" numeric(18,2) NOT NULL DEFAULT 0;
            """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                ALTER TABLE "Properties"
                ADD COLUMN IF NOT EXISTS "WaterPrice" numeric(18,2) NOT NULL DEFAULT 0;
            """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                ALTER TABLE "Properties"
                ADD COLUMN IF NOT EXISTS "ModerationStatus" integer NOT NULL DEFAULT 0;
            """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                ALTER TABLE "Properties"
                ADD COLUMN IF NOT EXISTS "AvailabilityStatus" integer NOT NULL DEFAULT 0;
            """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                ALTER TABLE "Properties"
                ADD COLUMN IF NOT EXISTS "ApprovedAt" timestamp with time zone NULL;
            """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                ALTER TABLE "Properties"
                ADD COLUMN IF NOT EXISTS "RejectedAt" timestamp with time zone NULL;
            """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                ALTER TABLE "Properties"
                ADD COLUMN IF NOT EXISTS "RejectionReason" character varying(500) NULL;
            """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                ALTER TABLE "Invoices"
                ADD COLUMN IF NOT EXISTS "OldElectricityReading" numeric(10,2) NULL;
            """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                ALTER TABLE "Invoices"
                ADD COLUMN IF NOT EXISTS "NewElectricityReading" numeric(10,2) NULL;
            """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                ALTER TABLE "Invoices"
                ADD COLUMN IF NOT EXISTS "OldWaterReading" numeric(10,2) NULL;
            """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                ALTER TABLE "Invoices"
                ADD COLUMN IF NOT EXISTS "NewWaterReading" numeric(10,2) NULL;
            """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                ALTER TABLE "Invoices"
                ADD COLUMN IF NOT EXISTS "ReceiptUrl" character varying(255) NULL;
            """);
        }

        private static async Task EnsureMigrationHistoryConsistencyAsync(AppDbContext dbContext)
        {
            await dbContext.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                    "MigrationId" character varying(150) NOT NULL,
                    "ProductVersion" character varying(32) NOT NULL,
                    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
                );
            """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                SELECT '20260503193419_InitialCreate', '10.0.5'
                WHERE EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = 'public' AND table_name = 'Amenities'
                )
                AND EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = 'public' AND table_name = 'Users'
                )
                AND EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = 'public' AND table_name = 'Areas'
                )
                AND NOT EXISTS (
                    SELECT 1
                    FROM "__EFMigrationsHistory"
                    WHERE "MigrationId" = '20260503193419_InitialCreate'
                );
            """);
        }
    }
}
