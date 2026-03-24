using Backend_Boarding_house_management_system.DTOs.Appointment.Requests;
using Backend_Boarding_house_management_system.DTOs.Appointment.Responses;
using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentResponse> GetAppointmentByIdAsync(GetAppointmentByIdRequest request);
        Task<AppointmentListResponse> GetAppointmentsByFilterAsync(
            EntityFilter<Appointment> filter,
            EntitySort<Appointment> sort,
            EntityPage page);
        Task<AppointmentResponse> CreateAppointmentAsync(CreateAppointmentRequest request);
        Task<bool> UpdateAppointmentAsync(UpdateAppointmentRequest request);
        Task<bool> DeleteAppointmentAsync(DeleteAppointmentRequest request);
    }
}
