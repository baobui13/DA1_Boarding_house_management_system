using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces.Base;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IViewHistoryRepository : IRepository<ViewHistory, string>
    {
        // Tất cả CRUD chung đã được kế thừa từ IRepository.

        /// <summary>
        /// Lấy lịch sử xem gần đây của user (kèm Property + RoomAmenities) để xây dựng preference cho recommendation.
        /// </summary>
        Task<List<ViewHistory>> GetRecentForUserAsync(string userId, int limit = 30);
    }
}
