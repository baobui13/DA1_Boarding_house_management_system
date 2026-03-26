using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(string id);
        Task<(IEnumerable<Payment>, int)> GetByFilterAsync(
            EntityFilter<Payment> filter,
            EntitySort<Payment> sort,
            EntityPage page);
        Task AddAsync(Payment entity);
        Task UpdateAsync(Payment entity);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}
