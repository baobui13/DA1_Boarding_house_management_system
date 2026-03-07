using Backend_Boarding_house_management_system.DTOs.Property.Requests;
using Backend_Boarding_house_management_system.DTOs.Property.Responses;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IPropertyService
    {
        Task<PropertyResponse> GetPropertyByIdAsync(GetPropertyByIdRequest request);
        Task<PropertyListResponse> GetPropertiesByFilterAsync(GetPropertiesByFilterRequest request);
        Task<PropertyResponse> CreatePropertyAsync(CreatePropertyRequest request);
        Task<bool> UpdatePropertyAsync(UpdatePropertyRequest request);
        Task<bool> DeletePropertyAsync(DeletePropertyRequest request);
    }
}
