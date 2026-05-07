using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Implements.Base;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Page;
using Plainquire.Sort;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class ComplaintRepository : Repository<Complaint, string>, IComplaintRepository
    {
        public ComplaintRepository(AppDbContext context) : base(context) { }

        private IQueryable<Complaint> GetDetailsQuery()
            => _dbSet.Include(c => c.Creator);

        public async Task<Complaint?> GetByIdWithDetailsAsync(string id)
            => await GetDetailsQuery().FirstOrDefaultAsync(c => c.Id == id);

        public async Task<(IEnumerable<Complaint> Items, int TotalCount)> GetByFilterWithDetailsAsync(
            EntityFilter<Complaint> filter,
            EntitySort<Complaint> sort,
            EntityPage page)
        {
            var query = GetDetailsQuery().AsNoTracking();
            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var items = await query.Page(page).ToListAsync();
            return (items, totalCount);
        }
    }
}
