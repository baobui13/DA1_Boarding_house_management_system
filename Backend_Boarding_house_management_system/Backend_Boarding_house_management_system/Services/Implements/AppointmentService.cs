using AutoMapper;
using Backend_Boarding_house_management_system.DTOs.Appointment.Requests;
using Backend_Boarding_house_management_system.DTOs.Appointment.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.Data;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;
using Microsoft.AspNetCore.SignalR;
using Backend_Boarding_house_management_system.Hubs;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IPropertyRepository propertyRepository,
            IUserRepository userRepository,
            AppDbContext context,
            IMapper mapper,
            IHubContext<ChatHub> hubContext)
        {
            _appointmentRepository = appointmentRepository;
            _propertyRepository = propertyRepository;
            _userRepository = userRepository;
            _context = context;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<AppointmentResponse> GetAppointmentByIdAsync(GetAppointmentByIdRequest request)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(request.Id);
            if (appointment == null)
                throw new NotFoundException($"Khong tim thay cuoc hen voi Id '{request.Id}'.");

            return _mapper.Map<AppointmentResponse>(appointment);
        }

        public async Task<AppointmentListResponse> GetAppointmentsByFilterAsync(
            EntityFilter<Appointment> filter,
            EntitySort<Appointment> sort,
            EntityPage page, string? landlordId = null)
        {
            var (appointments, totalCount) = await _appointmentRepository.GetAppointmentsWithLandlordFilterAsync(filter, sort, page, landlordId);

            return new AppointmentListResponse
            {
                Items = _mapper.Map<List<AppointmentResponse>>(appointments),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
        }

        public async Task<AppointmentResponse> CreateAppointmentAsync(CreateAppointmentRequest request)
        {
            if (request.AppointmentDateTime <= DateTime.UtcNow)
                throw new BadRequestException("Thời gian hẹn xem phòng phải ở thời điểm tương lai.");

            var property = await _propertyRepository.GetByIdAsync(request.PropertyId);
            if (property == null)
                throw new NotFoundException($"Khong tim thay phong voi Id '{request.PropertyId}'.");

            if (property.ModerationStatus != ModerationStatusEnum.Approved)
                throw new BadRequestException("Không thể đặt lịch hẹn xem phòng trọ chưa được duyệt.");

            if (property.AvailabilityStatus != AvailabilityStatusEnum.Available)
                throw new BadRequestException("Phòng trọ này hiện tại đã được thuê hoặc đang bảo trì.");

            var tenant = await _userRepository.GetByIdAsync(request.UserId);
            if (tenant == null)
                throw new NotFoundException($"Khong tim thay nguoi dung voi Id '{request.UserId}'.");

            var appointment = _mapper.Map<Appointment>(request);
            appointment.Id = Guid.NewGuid().ToString();
            appointment.CreatedAt = DateTime.UtcNow;

            await _appointmentRepository.AddAsync(appointment);

            // Tự động thông báo cho Chủ trọ khi có lịch hẹn xem phòng mới
            var tenantName = tenant.FullName ?? "Khách thuê";
            var localTimeCreate = appointment.AppointmentDateTime.AddHours(7);
            var landlordNotification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = property.LandlordId,
                Type = NotificationType.Appointment,
                Content = $"Có yêu cầu xem phòng mới từ {tenantName} cho {property.PropertyName} vào lúc {localTimeCreate:HH:mm dd/MM/yyyy}.",
                IsRead = false,
                Timestamp = DateTime.UtcNow,
                RelatedId = appointment.Id
            };
            _context.Notifications.Add(landlordNotification);
            await _context.SaveChangesAsync();

            var created = await _appointmentRepository.GetByIdAsync(appointment.Id);
            return _mapper.Map<AppointmentResponse>(created ?? appointment);
        }

        public async Task<bool> UpdateAppointmentAsync(UpdateAppointmentRequest request, string? currentUserId = null)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(request.Id);
            if (appointment == null)
                throw new NotFoundException($"Khong tim thay cuoc hen voi Id '{request.Id}'.");

            var oldStatus = appointment.Status;

            if (request.AppointmentDateTime.HasValue)
                appointment.AppointmentDateTime = request.AppointmentDateTime.Value;
                
            if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<AppointmentStatus>(request.Status, true, out var parsedStatus))
                appointment.Status = parsedStatus;
                
            if (request.Note != null)
                appointment.Note = request.Note;

            appointment.UpdatedAt = DateTime.UtcNow;

            await _appointmentRepository.UpdateAsync(appointment);

            // Convert to Vietnam Time (UTC+7) for display
            var localTime = appointment.AppointmentDateTime.AddHours(7);

            // Tự động thông báo cho Khách thuê khi trạng thái lịch hẹn thay đổi
            if (appointment.Status != oldStatus)
            {
                var property = await _propertyRepository.GetByIdAsync(appointment.PropertyId);
                var propName = property?.PropertyName ?? "phòng trọ";
                
                var statusMsg = appointment.Status switch
                {
                    AppointmentStatus.Confirmed => $"Lịch xem phòng {propName} lúc {localTime:HH:mm dd/MM/yyyy} đã được xác nhận.",
                    AppointmentStatus.Rejected => $"Lịch xem phòng {propName} lúc {localTime:HH:mm dd/MM/yyyy} đã bị từ chối.",
                    AppointmentStatus.Cancelled => $"Lịch xem phòng {propName} lúc {localTime:HH:mm dd/MM/yyyy} đã bị hủy.",
                    _ => $"Lịch xem phòng {propName} lúc {localTime:HH:mm dd/MM/yyyy} đã được cập nhật."
                };

                var tenantNotification = new Notification
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = appointment.UserId,
                    Type = NotificationType.Appointment,
                    Content = statusMsg,
                    IsRead = false,
                    Timestamp = DateTime.UtcNow,
                    RelatedId = appointment.Id
                };
                _context.Notifications.Add(tenantNotification);

                if (!string.IsNullOrEmpty(currentUserId))
                {
                    var receiverId = currentUserId == appointment.UserId ? property.LandlordId : appointment.UserId;
                    var message = new Message
                    {
                        Id = Guid.NewGuid().ToString(),
                        SenderId = currentUserId,
                        ReceiverId = receiverId,
                        Content = statusMsg,
                        IsRead = false,
                        Timestamp = DateTime.UtcNow,
                        PropertyId = appointment.PropertyId
                    };
                    _context.Messages.Add(message);
                    
                    await _hubContext.Clients.Group(receiverId).SendAsync("ReceiveMessage", currentUserId, statusMsg);
                }

                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> DeleteAppointmentAsync(DeleteAppointmentRequest request)
        {
            if (!await _appointmentRepository.ExistsAsync(request.Id))
                throw new NotFoundException($"Khong tim thay cuoc hen voi Id '{request.Id}'.");

            await _appointmentRepository.DeleteAsync(request.Id);
            return true;
        }
    }
}
