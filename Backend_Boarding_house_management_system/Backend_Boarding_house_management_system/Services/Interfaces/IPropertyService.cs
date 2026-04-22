using Backend_Boarding_house_management_system.DTOs.Property.Requests;
using Backend_Boarding_house_management_system.DTOs.Property.Responses;
using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IPropertyService
    {
        Task<PropertyResponse> GetPropertyByIdAsync(GetPropertyByIdRequest request);
        Task<PropertyDetailResponse> GetPropertyDetailByIdAsync(GetPropertyByIdRequest request);
        Task<PropertyListResponse> GetPropertiesByFilterAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page);
        Task<PropertyResponse> CreatePropertyAsync(CreatePropertyRequest request);
        Task<bool> UpdatePropertyAsync(UpdatePropertyRequest request);
        Task<bool> DeletePropertyAsync(DeletePropertyRequest request);
    }
}
