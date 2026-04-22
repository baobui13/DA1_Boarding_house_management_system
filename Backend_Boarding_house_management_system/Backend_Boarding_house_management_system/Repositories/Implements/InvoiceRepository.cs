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
    public class InvoiceRepository : Repository<Invoice, string>, IInvoiceRepository
    {
        public InvoiceRepository(AppDbContext context) : base(context) { }

        private IQueryable<Invoice> GetDetailsQuery()
            => _dbSet
                .Include(i => i.Contract)
                    .ThenInclude(c => c.Property)
                .Include(i => i.Contract)
                    .ThenInclude(c => c.Tenant)
                .Include(i => i.Payments);

        public async Task<Invoice?> GetByIdWithDetailsAsync(string id)
            => await GetDetailsQuery().FirstOrDefaultAsync(i => i.Id == id);

        public async Task<(IEnumerable<Invoice> Items, int TotalCount)> GetByFilterWithDetailsAsync(
            EntityFilter<Invoice> filter,
            EntitySort<Invoice> sort,
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
