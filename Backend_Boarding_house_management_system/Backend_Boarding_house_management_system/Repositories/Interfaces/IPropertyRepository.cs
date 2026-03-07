using Backend_Boarding_house_management_system.Entities;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IPropertyRepository
    {
        Task<Property?> GetPropertyByIdAsync(string id);
        Task<(IEnumerable<Property>, int)> GetPropertiesByFilterAsync(string? landlordId, string? areaId, string? propertyName, string? address, decimal? minPrice, decimal? maxPrice, string? status, decimal? minSize, decimal? maxSize, DateTime? createdAfter, DateTime? createdBefore, string sortBy, bool isDescending, int pageNumber, int pageSize);
        Task<Property> CreatePropertyAsync(Property property);
        Task<bool> UpdatePropertyAsync(Property property);
        Task<bool> DeletePropertyAsync(string propertyId);
    }
}
