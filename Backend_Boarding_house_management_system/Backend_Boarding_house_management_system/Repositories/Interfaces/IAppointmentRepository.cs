using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<Appointment?> GetAppointmentByIdAsync(string id);
        Task<(IEnumerable<Appointment>, int)> GetAppointmentsByFilterAsync(
            EntityFilter<Appointment> filter,
            EntitySort<Appointment> sort,
            EntityPage page);
        Task<Appointment> CreateAppointmentAsync(Appointment appointment);
        Task<bool> UpdateAppointmentAsync(Appointment appointment);
        Task<bool> DeleteAppointmentAsync(string id);
    }
}
