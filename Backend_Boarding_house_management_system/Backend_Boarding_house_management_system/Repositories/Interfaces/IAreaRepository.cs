using Backend_Boarding_house_management_system.Entities;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IAreaRepository
    {
        Task<Area?> GetAreaByIdAsync(string id);
        Task<(IEnumerable<Area>, int)> GetAreasByFilterAsync(string? name, string? address, string? landlordId, DateTime? createdAfter, DateTime? createdBefore, string sortBy, bool isDescending, int pageNumber, int pageSize);
        Task<bool> UpdateAreaAsync(Area area);
        Task<bool> DeleteAreaAsync(string areaId);
        Task<Area> CreateAreaAsync(Area area);
    }
}
