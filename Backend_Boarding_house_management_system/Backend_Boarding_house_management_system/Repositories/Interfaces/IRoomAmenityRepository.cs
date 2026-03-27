using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IRoomAmenityRepository
    {
        Task<RoomAmenity?> GetByIdAsync(string id);
        Task<(IEnumerable<RoomAmenity>, int)> GetByFilterAsync(
            EntityFilter<RoomAmenity> filter,
            EntitySort<RoomAmenity> sort,
            EntityPage page);
        Task AddAsync(RoomAmenity entity);
        Task UpdateAsync(RoomAmenity entity);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<bool> ExistsForRoomAndAmenityAsync(string roomId, string amenityId);
    }
}
