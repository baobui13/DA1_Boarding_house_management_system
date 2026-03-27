using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface ITenantDocumentRepository
    {
        Task<TenantDocument?> GetByIdAsync(string id);
        Task<(IEnumerable<TenantDocument>, int)> GetByFilterAsync(
            EntityFilter<TenantDocument> filter,
            EntitySort<TenantDocument> sort,
            EntityPage page);
        Task AddAsync(TenantDocument entity);
        Task UpdateAsync(TenantDocument entity);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}
