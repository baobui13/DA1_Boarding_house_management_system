using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Backend_Boarding_house_management_system.Data;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class AreaRepository : IAreaRepository
    {
        private readonly AppDbContext _context;
        public AreaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Area?> GetAreaByIdAsync(string id)
        {
            return await _context.Areas.FindAsync(id);
        }

        public async Task<(IEnumerable<Area>, int)> GetAreasByFilterAsync(
            EntityFilter<Area> filter,
            EntitySort<Area> sort,
            EntityPage page)
        {
            var query = _context.Areas.AsNoTracking();

            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var areas = await query.Page(page).ToListAsync();

            return (areas, totalCount);
        }

        public async Task<bool> UpdateAreaAsync(Area area)
        {
            _context.Areas.Update(area);
            var result = await _context.SaveChangesAsync() > 0;
            return result;
        }

        public async Task<bool> DeleteAreaAsync(string areaId)
        {
            var area = await _context.Areas.FindAsync(areaId);
            if (area == null)
                return false;
            _context.Areas.Remove(area);
            var result = await _context.SaveChangesAsync() > 0;
            return result;
        }

        public async Task<Area> CreateAreaAsync(Area area)
        {
            _context.Areas.Add(area);
            var result = await _context.SaveChangesAsync() > 0;
            return result ? area : null!;
        }
    }
}
