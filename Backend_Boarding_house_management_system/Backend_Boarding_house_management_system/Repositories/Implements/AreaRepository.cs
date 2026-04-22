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
    public class AreaRepository : Repository<Area, string>, IAreaRepository
    {
        public AreaRepository(AppDbContext context) : base(context) { }

        private IQueryable<Area> GetDetailsQuery()
            => _dbSet
                .Include(a => a.Landlord)
                .Include(a => a.Properties);

        public async Task<Area?> GetByIdWithDetailsAsync(string id)
            => await GetDetailsQuery().FirstOrDefaultAsync(a => a.Id == id);

        public async Task<(IEnumerable<Area> Items, int TotalCount)> GetByFilterWithDetailsAsync(
            EntityFilter<Area> filter,
            EntitySort<Area> sort,
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
