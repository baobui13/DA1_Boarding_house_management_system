using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces.Base;
using Plainquire.Filter;
using Plainquire.Page;
using Plainquire.Sort;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IPropertyImageRepository : IRepository<PropertyImage, string>
    {
        Task<PropertyImage?> GetByIdWithDetailsAsync(string id);
        Task<(IEnumerable<PropertyImage> Items, int TotalCount)> GetByFilterWithDetailsAsync(
            EntityFilter<PropertyImage> filter,
            EntitySort<PropertyImage> sort,
            EntityPage page);
    }
}
