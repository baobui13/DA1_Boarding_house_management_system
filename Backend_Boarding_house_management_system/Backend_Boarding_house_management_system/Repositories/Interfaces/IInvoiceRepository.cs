using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces.Base;
using Plainquire.Filter;
using Plainquire.Page;
using Plainquire.Sort;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IInvoiceRepository : IRepository<Invoice, string>
    {
        Task<Invoice?> GetByIdWithDetailsAsync(string id);
        Task<(IEnumerable<Invoice> Items, int TotalCount)> GetByFilterWithDetailsAsync(
            EntityFilter<Invoice> filter,
            EntitySort<Invoice> sort,
            EntityPage page);
    }
}
