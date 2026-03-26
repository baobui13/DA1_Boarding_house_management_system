using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(string id);
        Task<(IEnumerable<Notification>, int)> GetByFilterAsync(
            EntityFilter<Notification> filter,
            EntitySort<Notification> sort,
            EntityPage page);
        Task AddAsync(Notification entity);
        Task UpdateAsync(Notification entity);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}
