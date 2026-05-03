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
using Backend_Boarding_house_management_system.DTOs.Authentication.Requests;
using Backend_Boarding_house_management_system.DTOs.Authentication.Responses;
using Backend_Boarding_house_management_system.DTOs.Contract.Requests;
using Backend_Boarding_house_management_system.DTOs.Contract.Responses;
using Backend_Boarding_house_management_system.DTOs.Invoice.Requests;
using Backend_Boarding_house_management_system.DTOs.Invoice.Responses;
using Backend_Boarding_house_management_system.DTOs.Message.Requests;
using Backend_Boarding_house_management_system.DTOs.Message.Responses;
using Backend_Boarding_house_management_system.DTOs.Notification.Requests;
using Backend_Boarding_house_management_system.DTOs.Notification.Responses;
using Backend_Boarding_house_management_system.DTOs.Payment.Requests;
using Backend_Boarding_house_management_system.DTOs.Payment.Responses;
using Backend_Boarding_house_management_system.DTOs.TenantDocument.Requests;
using Backend_Boarding_house_management_system.DTOs.TenantDocument.Responses;
using Backend_Boarding_house_management_system.DTOs.SearchHistory.Requests;
using Backend_Boarding_house_management_system.DTOs.SearchHistory.Responses;
using Backend_Boarding_house_management_system.DTOs.ViewHistory.Requests;
using Backend_Boarding_house_management_system.DTOs.ViewHistory.Responses;
using Backend_Boarding_house_management_system.DTOs.Rating.Requests;
using Backend_Boarding_house_management_system.DTOs.Rating.Responses;
using Backend_Boarding_house_management_system.DTOs.Complaint.Requests;
using Backend_Boarding_house_management_system.DTOs.Complaint.Responses;
using Backend_Boarding_house_management_system.Entities;

namespace Backend_Boarding_house_management_system.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Auth mappings
            CreateMap<User, AuthResponse>();
            CreateMap<RegisterRequest, User>();

            // User mappings
            CreateMap<User, UserResponse>()
                .ForMember(
                    dest => dest.Status,
                    opt => opt.MapFrom(src =>
                        src.LockoutEnabled && src.LockoutEnd.HasValue && src.LockoutEnd.Value > DateTimeOffset.UtcNow
                            ? "Blocked"
                            : "Active"))
                .ForMember(
                    dest => dest.IsBlocked,
                    opt => opt.MapFrom(src =>
                        src.LockoutEnabled && src.LockoutEnd.HasValue && src.LockoutEnd.Value > DateTimeOffset.UtcNow));
            CreateMap<UpdateUserRequest, User>();
            
            // Area mappings
            CreateMap<Area, AreaResponse>();
            CreateMap<Area, AreaDetailResponse>();
            CreateMap<CreateAreaRequest, Area>();
            CreateMap<UpdateAreaRequest, Area>();
            
            // Property mappings
            CreateMap<Property, PropertyResponse>();
            CreateMap<Property, PropertyDetailResponse>();
            CreateMap<CreatePropertyRequest, Property>()
                .ForMember(dest => dest.ModerationStatus, opt => opt.MapFrom(src => ModerationStatusEnum.Pending))
                .ForMember(dest => dest.AvailabilityStatus, opt => opt.MapFrom(src => AvailabilityStatusEnum.Available));
            CreateMap<UpdatePropertyRequest, Property>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // PropertyImage mappings
            CreateMap<PropertyImage, PropertyImageResponse>();
            CreateMap<PropertyImage, PropertyImageDetailResponse>();
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

            // Contract mappings
            CreateMap<Contract, ContractResponse>();
            CreateMap<CreateContractRequest, Contract>();
            CreateMap<UpdateContractRequest, Contract>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Invoice mappings
            CreateMap<Invoice, InvoiceResponse>();
            CreateMap<Invoice, InvoiceDetailResponse>()
                .ForMember(dest => dest.Property, opt => opt.MapFrom(src => src.Contract != null ? src.Contract.Property : null))
                .ForMember(dest => dest.Tenant, opt => opt.MapFrom(src => src.Contract != null ? src.Contract.Tenant : null));
            CreateMap<CreateInvoiceRequest, Invoice>();
            CreateMap<UpdateInvoiceRequest, Invoice>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Message mappings
            CreateMap<Message, MessageResponse>();
            CreateMap<CreateMessageRequest, Message>();
            CreateMap<UpdateMessageRequest, Message>();

            // Notification mappings
            CreateMap<Notification, NotificationResponse>();
            CreateMap<CreateNotificationRequest, Notification>();
            CreateMap<UpdateNotificationRequest, Notification>();

            // Payment mappings
            CreateMap<Payment, PaymentResponse>();
            CreateMap<Payment, PaymentDetailResponse>()
                .ForMember(dest => dest.Contract, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.Contract : null))
                .ForMember(dest => dest.Property, opt => opt.MapFrom(src => src.Invoice != null && src.Invoice.Contract != null ? src.Invoice.Contract.Property : null))
                .ForMember(dest => dest.Tenant, opt => opt.MapFrom(src => src.Invoice != null && src.Invoice.Contract != null ? src.Invoice.Contract.Tenant : null));
            CreateMap<CreatePaymentRequest, Payment>();

            // TenantDocument mappings
            CreateMap<TenantDocument, TenantDocumentResponse>();
            CreateMap<CreateTenantDocumentRequest, TenantDocument>();
            CreateMap<UpdateTenantDocumentRequest, TenantDocument>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // SearchHistory mappings
            CreateMap<SearchHistory, SearchHistoryResponse>();
            CreateMap<CreateSearchHistoryRequest, SearchHistory>();

            // ViewHistory mappings
            CreateMap<ViewHistory, ViewHistoryResponse>();
            CreateMap<CreateViewHistoryRequest, ViewHistory>();

            // Rating mappings
            CreateMap<Rating, RatingResponse>();
            CreateMap<Rating, RatingDetailResponse>();
            CreateMap<CreateRatingRequest, Rating>();
            CreateMap<UpdateRatingRequest, Rating>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Complaint mappings
            CreateMap<Complaint, ComplaintResponse>();
            CreateMap<Complaint, ComplaintDetailResponse>();
            CreateMap<CreateComplaintRequest, Complaint>();
            CreateMap<UpdateComplaintRequest, Complaint>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
