using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Extensions;
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

// Đăng ký cấu hình Database
builder.Services.AddDatabaseServices(builder.Configuration);

// Đăng ký các Repository và Service của ứng dụng
builder.Services.AddApplicationRepositoriesAndServices();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var frontendOrigins = new[]
{
    "http://localhost:3000",
    "http://127.0.0.1:3000",
    "http://localhost:4173",
    "http://127.0.0.1:4173",
    "http://localhost:5173",
    "http://127.0.0.1:5173",
    "http://localhost:5174",
    "http://127.0.0.1:5174",
    "http://localhost:5175",
    "http://127.0.0.1:5175"
};

// Cấu hình CORS cho frontend local/dev
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(frontendOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

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

// Đăng ký cấu hình authentication bằng extension
builder.Services.AddAuthenticationServices(builder.Configuration);


// ===========================================================================================


var app = builder.Build();

// Kiểm tra kết nối tới Database khi ứng dụng khởi động
await app.CheckDatabaseConnectionAsync();

// Kiểm tra kết nối tới Cloudinary khi ứng dụng khởi động
await app.CheckCloudinaryConnectionAsync();

// Kiểm tra kết nối tới các dịch vụ authentication (JWT & Google) khi ứng dụng khởi động
app.CheckAuthenticationConnections();

await SyncPropertyAvailabilityStatusesAsync(app.Services);

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

app.Run();

static async Task SyncPropertyAvailabilityStatusesAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var occupiedPropertyIds = await db.Contracts
        .AsNoTracking()
        .Where(contract =>
            contract.Status == "Active" ||
            contract.Status == "NearExpiry" ||
            contract.Status == "Signed" ||
            contract.Status == "Approved")
        .Select(contract => contract.PropertyId)
        .Distinct()
        .ToListAsync();

    var occupiedSet = occupiedPropertyIds.ToHashSet(StringComparer.Ordinal);
    var properties = await db.Properties.ToListAsync();
    var hasChanges = false;

    foreach (var property in properties)
    {
        if (occupiedSet.Contains(property.Id))
        {
            if (property.AvailabilityStatus != AvailabilityStatusEnum.Rented)
            {
                property.AvailabilityStatus = AvailabilityStatusEnum.Rented;
                property.UpdatedAt = DateTime.UtcNow;
                hasChanges = true;
            }
            continue;
        }

        if (property.AvailabilityStatus == AvailabilityStatusEnum.Rented)
        {
            property.AvailabilityStatus = AvailabilityStatusEnum.Available;
            property.UpdatedAt = DateTime.UtcNow;
            hasChanges = true;
        }
    }

    if (hasChanges)
    {
        await db.SaveChangesAsync();
    }
}
