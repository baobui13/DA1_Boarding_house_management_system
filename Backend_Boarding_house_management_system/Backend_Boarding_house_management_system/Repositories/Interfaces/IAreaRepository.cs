using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IAreaRepository
    {
        Task<Area?> GetAreaByIdAsync(string id);
        Task<(IEnumerable<Area>, int)> GetAreasByFilterAsync(
            EntityFilter<Area> filter,
            EntitySort<Area> sort,
            EntityPage page);
        Task<bool> UpdateAreaAsync(Area area);
        Task<bool> DeleteAreaAsync(string areaId);
        Task<Area> CreateAreaAsync(Area area);
    }
}
