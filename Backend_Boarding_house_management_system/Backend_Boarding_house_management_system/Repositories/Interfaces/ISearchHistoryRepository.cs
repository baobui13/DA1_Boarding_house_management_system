using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface ISearchHistoryRepository
    {
        Task<SearchHistory?> GetByIdAsync(string id);
        Task<(IEnumerable<SearchHistory>, int)> GetByFilterAsync(
            EntityFilter<SearchHistory> filter,
            EntitySort<SearchHistory> sort,
            EntityPage page);
        Task AddAsync(SearchHistory entity);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}
