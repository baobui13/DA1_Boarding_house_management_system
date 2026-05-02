using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces.Base;
using Plainquire.Filter;
using Plainquire.Page;
using Plainquire.Sort;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IRatingRepository : IRepository<Rating, string>
    {
        Task<Rating?> GetByIdWithDetailsAsync(string id);
        Task<(IEnumerable<Rating> Items, int TotalCount)> GetByFilterWithDetailsAsync(
            EntityFilter<Rating> filter,
            EntitySort<Rating> sort,
            EntityPage page);
        Task<bool> ExistsByTenantAndPropertyAsync(string tenantId, string propertyId, string? excludeId = null);
    }
}
