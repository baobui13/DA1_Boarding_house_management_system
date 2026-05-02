using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces.Base;
using Plainquire.Filter;
using Plainquire.Page;
using Plainquire.Sort;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IComplaintRepository : IRepository<Complaint, string>
    {
        Task<Complaint?> GetByIdWithDetailsAsync(string id);
        Task<(IEnumerable<Complaint> Items, int TotalCount)> GetByFilterWithDetailsAsync(
            EntityFilter<Complaint> filter,
            EntitySort<Complaint> sort,
            EntityPage page);
    }
}
