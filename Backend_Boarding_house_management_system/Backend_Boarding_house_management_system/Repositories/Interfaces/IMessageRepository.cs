using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IMessageRepository
    {
        Task<Message?> GetByIdAsync(string id);
        Task<(IEnumerable<Message>, int)> GetByFilterAsync(
            EntityFilter<Message> filter,
            EntitySort<Message> sort,
            EntityPage page);
        Task AddAsync(Message entity);
        Task UpdateAsync(Message entity);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}
