using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class ViewHistoryRepository : IViewHistoryRepository
    {
        private readonly AppDbContext _context;
        public ViewHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ViewHistory?> GetByIdAsync(string id)
        {
            return await _context.ViewHistories
                .Include(vh => vh.User)
                .Include(vh => vh.Property)
                .FirstOrDefaultAsync(vh => vh.Id == id);
        }

        public async Task<(IEnumerable<ViewHistory>, int)> GetByFilterAsync(
            EntityFilter<ViewHistory> filter,
            EntitySort<ViewHistory> sort,
            EntityPage page)
        {
            var query = _context.ViewHistories
                .Include(vh => vh.User)
                .Include(vh => vh.Property)
                .AsNoTracking();

            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var items = await query.Page(page).ToListAsync();

            return (items, totalCount);
        }

        public async Task AddAsync(ViewHistory entity)
        {
            _context.ViewHistories.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _context.ViewHistories.FindAsync(id);
            if (entity != null)
            {
                _context.ViewHistories.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.ViewHistories.AnyAsync(vh => vh.Id == id);
        }
    }
}
