using AutoMapper;
using Backend_Boarding_house_management_system.DTOs.Appointment.Requests;
using Backend_Boarding_house_management_system.DTOs.Appointment.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IMapper _mapper;

        public AppointmentService(IAppointmentRepository appointmentRepository, IMapper mapper)
        {
            _appointmentRepository = appointmentRepository;
            _mapper = mapper;
        }

        public async Task<AppointmentResponse> GetAppointmentByIdAsync(GetAppointmentByIdRequest request)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(request.Id);
            if (appointment == null)
                throw new NotFoundException($"Không tìm thấy cuộc hẹn với Id '{request.Id}'.");

            return _mapper.Map<AppointmentResponse>(appointment);
        }

        public async Task<AppointmentListResponse> GetAppointmentsByFilterAsync(
            EntityFilter<Appointment> filter,
            EntitySort<Appointment> sort,
            EntityPage page)
        {
            var (appointments, totalCount) = await _appointmentRepository.GetAppointmentsByFilterAsync(filter, sort, page);

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
            var appointment = _mapper.Map<Appointment>(request);
            appointment.Id = Guid.NewGuid().ToString();
            appointment.CreatedAt = DateTime.UtcNow;

            var created = await _appointmentRepository.CreateAppointmentAsync(appointment);
            if (created == null)
                throw new BadRequestException("Tạo cuộc hẹn thất bại.");

            return _mapper.Map<AppointmentResponse>(created);
        }

        public async Task<bool> UpdateAppointmentAsync(UpdateAppointmentRequest request)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(request.Id);
            if (appointment == null)
                throw new NotFoundException($"Không tìm thấy cuộc hẹn với Id '{request.Id}'.");

            _mapper.Map(request, appointment);
            appointment.UpdatedAt = DateTime.UtcNow;

            var isSuccess = await _appointmentRepository.UpdateAppointmentAsync(appointment);
            if (!isSuccess)
                throw new BadRequestException("Cập nhật cuộc hẹn thất bại.");

            return true;
        }

        public async Task<bool> DeleteAppointmentAsync(DeleteAppointmentRequest request)
        {
            var isSuccess = await _appointmentRepository.DeleteAppointmentAsync(request.Id);
            if (!isSuccess)
                throw new NotFoundException($"Không tìm thấy hoặc xóa cuộc hẹn thất bại với Id '{request.Id}'.");

            return true;
        }
    }
}
