using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Repositories.Implements;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.Services.Implements;

namespace Backend_Boarding_house_management_system.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddApplicationRepositoriesAndServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<IAreaRepository, AreaRepository>();
            services.AddScoped<IAreaService, AreaService>();

            services.AddScoped<IPropertyRepository, PropertyRepository>();
            services.AddScoped<IPropertyService, PropertyService>();

            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IAmenityRepository, AmenityRepository>();
            services.AddScoped<IAmenityService, AmenityService>();

            services.AddScoped<IRoomAmenityRepository, RoomAmenityRepository>();
            services.AddScoped<IRoomAmenityService, RoomAmenityService>();
        }
    }
}
