using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces.Base;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IAppointmentRepository : IRepository<Appointment, string>
    {
        // Tất cả CRUD chung đã được kế thừa từ IRepository.
        // Thêm methods đặc thù của Appointment tại đây nếu cần.
    }
}
