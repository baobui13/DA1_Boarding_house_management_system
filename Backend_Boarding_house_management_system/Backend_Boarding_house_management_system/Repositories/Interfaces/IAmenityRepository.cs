using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.DTOs.Amenity.Requests;
using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IAmenityRepository
    {
        Task<Amenity?> GetByIdAsync(string id);
        Task<List<Amenity>> GetAllAsync();
        Task<PagedResponse<Amenity>> GetByFilterAsync(GetAmenitiesByFilterRequest request);
        Task AddAsync(Amenity amenity);
        Task UpdateAsync(Amenity amenity);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}
