using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Extensions;
using Backend_Boarding_house_management_system.Options;
using Backend_Boarding_house_management_system.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Plainquire.Filter.Mvc;
using Plainquire.Filter.Swashbuckle;
using Plainquire.Page.Mvc;
using Plainquire.Page.Swashbuckle;
using Plainquire.Sort.Mvc;
using Plainquire.Sort.Swashbuckle;
using System.Text;
using System.Text.Json.Serialization;
using AutoMapper;
using System.Linq;


var builder = WebApplication.CreateBuilder(args);

// In Development we force HTTP only by default.
// This avoids the very common Windows dev-certificate binding failures
// when Kestrel tries to listen on https://localhost:7134 (LocalhostListenOptions).
// Use the "http" launch profile, or set ASPNETCORE_URLS / launch profile "https"
// only when your dev cert is healthy.
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://localhost:5046");
}

// Đăng ký cấu hình Database
builder.Services.AddDatabaseServices(builder.Configuration);

// Đăng ký các Repository và Service của ứng dụng
builder.Services.AddApplicationRepositoriesAndServices();

// Đăng ký dịch vụ nền nhắc nhở thông báo (hóa đơn & hợp đồng sắp đến hạn)
builder.Services.AddHostedService<Backend_Boarding_house_management_system.Services.Implements.NotificationReminderBackgroundService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Cấu hình CORS (một lần duy nhất, với AllowCredentials cho auth)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:5174",
                "http://127.0.0.1:3000",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:5174")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();

builder.Services.AddControllers()
    .AddFilterSupport()
    .AddSortSupport()
    .AddPageSupport()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddSwaggerGen(options =>
{
    options.AddFilterSupport();
    options.AddSortSupport();
    options.AddPageSupport();
});

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(Program).Assembly);
});

// Bind cấu hình Cloudinary
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// Bind cấu hình ABSA Python service (two-head model)
builder.Services.Configure<AspectAnalysisOptions>(builder.Configuration.GetSection("AspectAnalysis"));

// Bind cấu hình Recommendation Scoring Engine (Hướng 1 - nhiều Profile/Mode)
builder.Services.Configure<RecommendationOptions>(builder.Configuration.GetSection(RecommendationOptions.SectionName));

// Đăng ký cấu hình authentication bằng extension
builder.Services.AddAuthenticationServices(builder.Configuration);

// Đăng ký Global Exception Handler (IExceptionHandler)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Cho phép services lấy thông tin user hiện tại từ HttpContext (để kiểm tra ownership)
builder.Services.AddHttpContextAccessor();


// ===========================================================================================


var app = builder.Build();

// Kiểm tra kết nối tới Database khi ứng dụng khởi động
await app.CheckDatabaseConnectionAsync();

// Kiểm tra kết nối tới Cloudinary khi ứng dụng khởi động
await app.CheckCloudinaryConnectionAsync();

// Kiểm tra kết nối tới Python ABSA microservice (non-fatal — keyword fallback exists)
await app.CheckAspectAnalysisConnectionAsync();

// Kiểm tra kết nối tới các dịch vụ authentication (JWT, Google, Facebook) khi ứng dụng khởi động
app.CheckAuthenticationConnections();

await SyncPropertyAvailabilityStatusesAsync(app.Services);

// Kích hoạt Global Exception Handler (phải đặt sớm trong pipeline)
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Phòng Trọ API v1");
        options.RoutePrefix = string.Empty; // Để Swagger hiện ra ngay khi chạy localhost (không cần gõ /swagger)
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHub<Backend_Boarding_house_management_system.Hubs.ChatHub>("/chathub");

app.Run();

static async Task SyncPropertyAvailabilityStatusesAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var occupiedPropertyIds = await db.Contracts
        .AsNoTracking()
        .Where(contract =>
            contract.Status == ContractStatus.Active ||
            contract.Status == ContractStatus.NearExpiry)
        .Select(contract => contract.PropertyId)
        .Distinct()
        .ToListAsync();

    var occupiedSet = occupiedPropertyIds.ToHashSet(StringComparer.Ordinal);
    var hasChanges = false;

    // Targeted updates: chỉ query những bản ghi cần thay đổi (tránh load toàn bộ bảng Properties)
    // 1. Các property đang được occupy (có contract active) mà chưa phải Rented
    if (occupiedPropertyIds.Count > 0)
    {
        var toRent = await db.Properties
            .Where(p => occupiedPropertyIds.Contains(p.Id) && p.AvailabilityStatus != AvailabilityStatusEnum.Rented)
            .ToListAsync();
        foreach (var p in toRent)
        {
            p.AvailabilityStatus = AvailabilityStatusEnum.Rented;
            p.UpdatedAt = DateTime.UtcNow;
            hasChanges = true;
        }
    }

    // 2. Các property đang Rented nhưng không còn contract active nào
    var currentlyRented = await db.Properties
        .Where(p => p.AvailabilityStatus == AvailabilityStatusEnum.Rented)
        .ToListAsync();
    foreach (var p in currentlyRented)
    {
        if (!occupiedSet.Contains(p.Id))
        {
            p.AvailabilityStatus = AvailabilityStatusEnum.Available;
            p.UpdatedAt = DateTime.UtcNow;
            hasChanges = true;
        }
    }

    if (hasChanges)
    {
        await db.SaveChangesAsync();
    }
}
