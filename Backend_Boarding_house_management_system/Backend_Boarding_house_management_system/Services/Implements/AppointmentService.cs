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
            var appointment = await _appointmentRepository.GetByIdAsync(request.Id);
            if (appointment == null)
                throw new NotFoundException($"Khong tim thay cuoc hen voi Id '{request.Id}'.");

            return _mapper.Map<AppointmentResponse>(appointment);
        }

        public async Task<AppointmentListResponse> GetAppointmentsByFilterAsync(
            EntityFilter<Appointment> filter,
            EntitySort<Appointment> sort,
            EntityPage page)
        {
            var (appointments, totalCount) = await _appointmentRepository.GetByFilterAsync(filter, sort, page);

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

            await _appointmentRepository.AddAsync(appointment);

            var created = await _appointmentRepository.GetByIdAsync(appointment.Id);
            return _mapper.Map<AppointmentResponse>(created ?? appointment);
        }

        public async Task<bool> UpdateAppointmentAsync(UpdateAppointmentRequest request)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(request.Id);
            if (appointment == null)
                throw new NotFoundException($"Khong tim thay cuoc hen voi Id '{request.Id}'.");

            _mapper.Map(request, appointment);
            appointment.UpdatedAt = DateTime.UtcNow;

            await _appointmentRepository.UpdateAsync(appointment);
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
