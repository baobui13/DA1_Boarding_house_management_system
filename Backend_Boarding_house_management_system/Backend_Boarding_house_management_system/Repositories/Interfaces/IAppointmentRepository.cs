using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces.Base;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IAppointmentRepository : IRepository<Appointment, string>
    {
        Task<(IEnumerable<Appointment> Items, int TotalCount)> GetAppointmentsWithLandlordFilterAsync(EntityFilter<Appointment> filter, EntitySort<Appointment> sort, EntityPage page, string? landlordId);
    }
}
