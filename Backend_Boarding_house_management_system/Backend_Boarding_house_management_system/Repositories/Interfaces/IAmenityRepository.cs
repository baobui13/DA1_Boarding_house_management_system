using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IAmenityRepository
    {
        Task<Amenity?> GetByIdAsync(string id);
        Task<List<Amenity>> GetAllAsync();
        Task<(IEnumerable<Amenity>, int)> GetByFilterAsync(
            EntityFilter<Amenity> filter,
            EntitySort<Amenity> sort,
            EntityPage page);
        Task AddAsync(Amenity amenity);
        Task UpdateAsync(Amenity amenity);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}
