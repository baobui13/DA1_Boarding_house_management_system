using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class SearchHistoryRepository : ISearchHistoryRepository
    {
        private readonly AppDbContext _context;
        public SearchHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SearchHistory?> GetByIdAsync(string id)
        {
            return await _context.SearchHistories
                .Include(sh => sh.User)
                .FirstOrDefaultAsync(sh => sh.Id == id);
        }

        public async Task<(IEnumerable<SearchHistory>, int)> GetByFilterAsync(
            EntityFilter<SearchHistory> filter,
            EntitySort<SearchHistory> sort,
            EntityPage page)
        {
            var query = _context.SearchHistories
                .Include(sh => sh.User)
                .AsNoTracking();

            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var items = await query.Page(page).ToListAsync();

            return (items, totalCount);
        }

        public async Task AddAsync(SearchHistory entity)
        {
            _context.SearchHistories.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _context.SearchHistories.FindAsync(id);
            if (entity != null)
            {
                _context.SearchHistories.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.SearchHistories.AnyAsync(sh => sh.Id == id);
        }
    }
}
