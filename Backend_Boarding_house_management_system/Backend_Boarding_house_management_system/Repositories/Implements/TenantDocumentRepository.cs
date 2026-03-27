using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class TenantDocumentRepository : ITenantDocumentRepository
    {
        private readonly AppDbContext _context;
        public TenantDocumentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TenantDocument?> GetByIdAsync(string id)
        {
            return await _context.TenantDocuments
                .Include(td => td.Tenant)
                .FirstOrDefaultAsync(td => td.Id == id);
        }

        public async Task<(IEnumerable<TenantDocument>, int)> GetByFilterAsync(
            EntityFilter<TenantDocument> filter,
            EntitySort<TenantDocument> sort,
            EntityPage page)
        {
            var query = _context.TenantDocuments
                .Include(td => td.Tenant)
                .AsNoTracking();

            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var items = await query.Page(page).ToListAsync();

            return (items, totalCount);
        }

        public async Task AddAsync(TenantDocument entity)
        {
            _context.TenantDocuments.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TenantDocument entity)
        {
            _context.TenantDocuments.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _context.TenantDocuments.FindAsync(id);
            if (entity != null)
            {
                _context.TenantDocuments.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.TenantDocuments.AnyAsync(td => td.Id == id);
        }
    }
}
