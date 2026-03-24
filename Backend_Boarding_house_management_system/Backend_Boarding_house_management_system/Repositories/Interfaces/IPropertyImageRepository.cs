using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IPropertyImageRepository
    {
        Task<PropertyImage?> GetPropertyImageByIdAsync(string id);
        Task<(IEnumerable<PropertyImage>, int)> GetPropertyImagesByFilterAsync(
            EntityFilter<PropertyImage> filter,
            EntitySort<PropertyImage> sort,
            EntityPage page);
        Task<PropertyImage> CreatePropertyImageAsync(PropertyImage propertyImage);
        Task<bool> UpdatePropertyImageAsync(PropertyImage propertyImage);
        Task<bool> DeletePropertyImageAsync(string propertyImageId);
    }
}
