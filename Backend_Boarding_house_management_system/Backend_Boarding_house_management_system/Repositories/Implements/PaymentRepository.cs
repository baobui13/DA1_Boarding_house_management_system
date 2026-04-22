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
    public class PaymentRepository : Repository<Payment, string>, IPaymentRepository
    {
        public PaymentRepository(AppDbContext context) : base(context) { }

        private IQueryable<Payment> GetDetailsQuery()
            => _dbSet
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Contract)
                        .ThenInclude(c => c.Property)
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Contract)
                        .ThenInclude(c => c.Tenant);

        public async Task<Payment?> GetByIdWithDetailsAsync(string id)
            => await GetDetailsQuery().FirstOrDefaultAsync(p => p.Id == id);

        public async Task<(IEnumerable<Payment> Items, int TotalCount)> GetByFilterWithDetailsAsync(
            EntityFilter<Payment> filter,
            EntitySort<Payment> sort,
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
