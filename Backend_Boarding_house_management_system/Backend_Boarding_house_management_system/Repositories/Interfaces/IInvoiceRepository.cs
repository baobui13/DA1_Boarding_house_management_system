using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<Invoice?> GetByIdAsync(string id);
        Task<(IEnumerable<Invoice>, int)> GetByFilterAsync(
            EntityFilter<Invoice> filter,
            EntitySort<Invoice> sort,
            EntityPage page);
        Task AddAsync(Invoice entity);
        Task UpdateAsync(Invoice entity);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}
