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
    public class RatingRepository : Repository<Rating, string>, IRatingRepository
    {
        public RatingRepository(AppDbContext context) : base(context) { }

        private IQueryable<Rating> GetDetailsQuery()
            => _dbSet
                .Include(r => r.Tenant)
                .Include(r => r.Property);

        public async Task<Rating?> GetByIdWithDetailsAsync(string id)
            => await GetDetailsQuery().FirstOrDefaultAsync(r => r.Id == id);

        public async Task<(IEnumerable<Rating> Items, int TotalCount)> GetByFilterWithDetailsAsync(
            EntityFilter<Rating> filter,
            EntitySort<Rating> sort,
            EntityPage page)
        {
            var query = GetDetailsQuery().AsNoTracking();
            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var items = await query.Page(page).ToListAsync();
            return (items, totalCount);
        }

        public async Task<bool> ExistsByTenantAndPropertyAsync(string tenantId, string propertyId, string? excludeId = null)
        {
            var query = _dbSet.Where(r => r.TenantId == tenantId && r.PropertyId == propertyId);
            if (!string.IsNullOrWhiteSpace(excludeId))
            {
                query = query.Where(r => r.Id != excludeId);
            }

            return await query.AnyAsync();
        }
    }
}
