using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Exceptions;

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


        public async Task<(IEnumerable<Area>, int)> GetAreasByFilterAsync(string? name, string? address, string? landlordId, DateTime? createdAfter, DateTime? createdBefore, string sortBy, bool isDescending, int pageNumber, int pageSize)
        {
            var query = _context.Areas.AsNoTracking().AsQueryable();
            if (!string.IsNullOrEmpty(name))
                query = query.Where(a => a.Name.Contains(name));

            if (!string.IsNullOrEmpty(address))
                query = query.Where(a => a.Address.Contains(address));

            if (!string.IsNullOrEmpty(landlordId))
                query = query.Where(a => a.LandlordId == landlordId);

            if (createdAfter.HasValue)
                query = query.Where(a => a.CreatedAt >= createdAfter.Value);

            if (createdBefore.HasValue)
                query = query.Where(a => a.CreatedAt <= createdBefore.Value);

            query = query.OrderBy($"{sortBy} {(isDescending ? "descending" : "ascending")}");

            var totalCount = await query.CountAsync();
            var areas = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

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
