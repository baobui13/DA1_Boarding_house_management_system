using Backend_Boarding_house_management_system.Entities;
using System.Threading.Tasks;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IPropertyImageRepository
    {
        Task<PropertyImage?> GetPropertyImageByIdAsync(string id);
        Task<(IEnumerable<PropertyImage>, int)> GetPropertyImagesByFilterAsync(string? propertyId, bool? isPrimary, string sortBy, bool isDescending, int pageNumber, int pageSize);
        Task<PropertyImage> CreatePropertyImageAsync(PropertyImage propertyImage);
        Task<bool> UpdatePropertyImageAsync(PropertyImage propertyImage);
        Task<bool> DeletePropertyImageAsync(string propertyImageId);
    }
}
