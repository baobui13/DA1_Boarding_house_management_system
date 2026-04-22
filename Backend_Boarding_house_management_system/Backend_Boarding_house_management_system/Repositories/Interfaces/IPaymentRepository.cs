using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces.Base;
using Plainquire.Filter;
using Plainquire.Page;
using Plainquire.Sort;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment, string>
    {
        Task<Payment?> GetByIdWithDetailsAsync(string id);
        Task<(IEnumerable<Payment> Items, int TotalCount)> GetByFilterWithDetailsAsync(
            EntityFilter<Payment> filter,
            EntitySort<Payment> sort,
            EntityPage page);
    }
}
