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


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký các Repository và Service của ứng dụng
builder.Services.AddApplicationRepositoriesAndServices();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

var app = builder.Build();

// Kiểm tra kết nối tới Cloudinary khi ứng dụng khởi động
await app.CheckCloudinaryConnectionAsync();

// Kiểm tra kết nối tới các dịch vụ authentication (JWT & Google) khi ứng dụng khởi động
app.CheckAuthenticationConnections();

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

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
