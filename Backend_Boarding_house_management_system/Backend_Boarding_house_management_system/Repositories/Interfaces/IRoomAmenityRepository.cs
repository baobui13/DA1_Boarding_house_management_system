using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces.Base;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IRoomAmenityRepository : IRepository<RoomAmenity, string>
    {
        /// <summary>Kiểm tra xem phòng đã có tiện ích này chưa.</summary>
        Task<bool> ExistsForPropertyAndAmenityAsync(string propertyId, string amenityId);
    }
}
