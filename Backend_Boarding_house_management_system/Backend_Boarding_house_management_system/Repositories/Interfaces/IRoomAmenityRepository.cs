using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.DTOs.RoomAmenity.Requests;
using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IRoomAmenityRepository
    {
        Task<RoomAmenity?> GetByIdAsync(string id);
        Task<PagedResponse<RoomAmenity>> GetByFilterAsync(GetRoomAmenitiesByFilterRequest request);
        Task AddAsync(RoomAmenity entity);
        Task UpdateAsync(RoomAmenity entity);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<bool> ExistsForRoomAndAmenityAsync(string roomId, string amenityId);
    }
}
