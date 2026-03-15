using Backend_Boarding_house_management_system.DTOs.Amenity.Requests;
using Backend_Boarding_house_management_system.DTOs.Amenity.Responses;
using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IAmenityService
    {
        Task<AmenityResponse> GetByIdAsync(string id);
        Task<AmenityPagedResponse> GetByFilterAsync(GetAmenitiesByFilterRequest request);
        Task<List<AmenityResponse>> GetAllAsync();
        Task AddAsync(CreateAmenityRequest request);
        Task UpdateAsync(UpdateAmenityRequest request);
        Task DeleteAsync(string id);
    }
}
