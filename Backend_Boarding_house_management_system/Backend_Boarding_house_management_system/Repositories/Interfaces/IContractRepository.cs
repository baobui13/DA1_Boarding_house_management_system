using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IContractRepository
    {
        Task<Contract?> GetByIdAsync(string id);
        Task<(IEnumerable<Contract>, int)> GetByFilterAsync(
            EntityFilter<Contract> filter,
            EntitySort<Contract> sort,
            EntityPage page);
        Task AddAsync(Contract entity);
        Task UpdateAsync(Contract entity);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}
