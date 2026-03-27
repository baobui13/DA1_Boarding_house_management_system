using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDbContext _context;
        public InvoiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice?> GetByIdAsync(string id)
        {
            return await _context.Invoices
                .Include(i => i.Contract)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<(IEnumerable<Invoice>, int)> GetByFilterAsync(
            EntityFilter<Invoice> filter,
            EntitySort<Invoice> sort,
            EntityPage page)
        {
            var query = _context.Invoices
                .Include(i => i.Contract)
                .AsNoTracking();

            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var items = await query.Page(page).ToListAsync();

            return (items, totalCount);
        }

        public async Task AddAsync(Invoice entity)
        {
            _context.Invoices.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Invoice entity)
        {
            _context.Invoices.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _context.Invoices.FindAsync(id);
            if (entity != null)
            {
                _context.Invoices.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Invoices.AnyAsync(i => i.Id == id);
        }
    }
}
