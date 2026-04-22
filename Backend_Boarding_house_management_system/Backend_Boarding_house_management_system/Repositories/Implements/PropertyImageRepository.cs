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
    public class PropertyImageRepository : Repository<PropertyImage, string>, IPropertyImageRepository
    {
        public PropertyImageRepository(AppDbContext context) : base(context) { }

        private IQueryable<PropertyImage> GetDetailsQuery()
            => _dbSet.Include(pi => pi.Property);

        public async Task<PropertyImage?> GetByIdWithDetailsAsync(string id)
            => await GetDetailsQuery().FirstOrDefaultAsync(pi => pi.Id == id);

        public async Task<(IEnumerable<PropertyImage> Items, int TotalCount)> GetByFilterWithDetailsAsync(
            EntityFilter<PropertyImage> filter,
            EntitySort<PropertyImage> sort,
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
