using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Repositories.Implements;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.Services.Implements;
using Backend_Boarding_house_management_system.Services.Implements.Recommendation;

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
            services.AddScoped<IPropertyImageRepository, PropertyImageRepository>();
            services.AddScoped<IPropertyImageService, PropertyImageService>();

            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IAmenityRepository, AmenityRepository>();

            services.AddScoped<IAmenityService, AmenityService>();

            services.AddScoped<IRoomAmenityRepository, RoomAmenityRepository>();
            services.AddScoped<IRoomAmenityService, RoomAmenityService>();

            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<IAppointmentService, AppointmentService>();

            services.AddScoped<IContractRepository, ContractRepository>();
            services.AddScoped<IContractService, ContractService>();

            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<IInvoiceService, InvoiceService>();

            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IMessageService, MessageService>();

            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<INotificationService, NotificationService>();

            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IPaymentService, PaymentService>();

            services.AddScoped<ITenantDocumentRepository, TenantDocumentRepository>();
            services.AddScoped<ITenantDocumentService, TenantDocumentService>();

            services.AddScoped<ISearchHistoryRepository, SearchHistoryRepository>();
            services.AddScoped<ISearchHistoryService, SearchHistoryService>();

            services.AddScoped<IViewHistoryRepository, ViewHistoryRepository>();
            services.AddScoped<IViewHistoryService, ViewHistoryService>();

            services.AddScoped<IRatingRepository, RatingRepository>();
            services.AddScoped<IRatingService, RatingService>();
            services.AddScoped<IAspectAnalysisService, AspectAnalysisService>();

            // Recommendation Scoring Engine (Hướng 1 - tách biệt scoring + hỗ trợ nhiều Profile/Mode)
            services.AddScoped<IPropertyScorer, ProfileBasedPropertyScorer>();

            // Named HttpClient for the external Python ABSA (two-head PhoBERT) microservice.
            // Actual timeout + base address logic lives in AspectAnalysisService (uses IOptions + per-request timeout).
            services.AddHttpClient("AbsaClient", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15); // hard safety cap; real value comes from options
            });

            services.AddScoped<IComplaintRepository, ComplaintRepository>();
            services.AddScoped<IComplaintService, ComplaintService>();

            // AI Chatbot Service
            services.AddHttpClient("AiService", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            services.AddScoped<IChatbotService, ChatbotService>();
        }
    }
}
