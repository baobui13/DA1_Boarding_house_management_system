using AutoMapper;
using Backend_Boarding_house_management_system.DTOs.Area.Requests;
using Backend_Boarding_house_management_system.DTOs.Area.Responses;
using Backend_Boarding_house_management_system.DTOs.Property.Requests;
using Backend_Boarding_house_management_system.DTOs.Property.Responses;
using Backend_Boarding_house_management_system.DTOs.User.Requests;
using Backend_Boarding_house_management_system.DTOs.User.Responses;
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
        }
    }
}
