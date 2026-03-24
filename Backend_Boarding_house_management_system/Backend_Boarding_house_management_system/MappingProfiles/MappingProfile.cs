using AutoMapper;
using Backend_Boarding_house_management_system.DTOs.Area.Requests;
using Backend_Boarding_house_management_system.DTOs.Area.Responses;
using Backend_Boarding_house_management_system.DTOs.Property.Requests;
using Backend_Boarding_house_management_system.DTOs.Property.Responses;
using Backend_Boarding_house_management_system.DTOs.User.Requests;
using Backend_Boarding_house_management_system.DTOs.User.Responses;
using Backend_Boarding_house_management_system.DTOs.PropertyImage.Requests;
using Backend_Boarding_house_management_system.DTOs.PropertyImage.Responses;
using Backend_Boarding_house_management_system.DTOs.Amenity.Requests;
using Backend_Boarding_house_management_system.DTOs.Amenity.Responses;
using Backend_Boarding_house_management_system.DTOs.RoomAmenity.Requests;
using Backend_Boarding_house_management_system.DTOs.RoomAmenity.Responses;
using Backend_Boarding_house_management_system.DTOs.Appointment.Requests;
using Backend_Boarding_house_management_system.DTOs.Appointment.Responses;
using Backend_Boarding_house_management_system.Entities;

namespace Backend_Boarding_house_management_system.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserResponse>();
            CreateMap<UpdateUserRequest, User>();
            
            // Area mappings
            CreateMap<Area, AreaResponse>();
            CreateMap<CreateAreaRequest, Area>();
            CreateMap<UpdateAreaRequest, Area>();
            
            // Property mappings
            CreateMap<Property, PropertyResponse>();
            CreateMap<CreatePropertyRequest, Property>();
            CreateMap<UpdatePropertyRequest, Property>();

            // PropertyImage mappings
            CreateMap<PropertyImage, PropertyImageResponse>();
            CreateMap<CreatePropertyImageRequest, PropertyImage>();

            // Amenity mappings
            CreateMap<Amenity, AmenityResponse>();
            CreateMap<CreateAmenityRequest, Amenity>();
            CreateMap<UpdateAmenityRequest, Amenity>();

            // RoomAmenity mappings
            CreateMap<RoomAmenity, RoomAmenityResponse>()
                .ForMember(dest => dest.AmenityName, opt => opt.MapFrom(src => src.Amenity != null ? src.Amenity.Name : string.Empty));
            CreateMap<CreateRoomAmenityRequest, RoomAmenity>();
            CreateMap<UpdateRoomAmenityRequest, RoomAmenity>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Appointment mappings
            CreateMap<Appointment, AppointmentResponse>();
            CreateMap<CreateAppointmentRequest, Appointment>();
            CreateMap<UpdateAppointmentRequest, Appointment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
