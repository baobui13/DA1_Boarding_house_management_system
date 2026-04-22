using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces.Base;
using Plainquire.Filter;
using Plainquire.Page;
using Plainquire.Sort;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IAreaRepository : IRepository<Area, string>
    {
        Task<Area?> GetByIdWithDetailsAsync(string id);
        Task<(IEnumerable<Area> Items, int TotalCount)> GetByFilterWithDetailsAsync(
            EntityFilter<Area> filter,
            EntitySort<Area> sort,
            EntityPage page);
    }
}
