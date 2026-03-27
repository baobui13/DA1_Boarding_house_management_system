using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IViewHistoryRepository
    {
        Task<ViewHistory?> GetByIdAsync(string id);
        Task<(IEnumerable<ViewHistory>, int)> GetByFilterAsync(
            EntityFilter<ViewHistory> filter,
            EntitySort<ViewHistory> sort,
            EntityPage page);
        Task AddAsync(ViewHistory entity);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}
