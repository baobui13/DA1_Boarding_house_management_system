using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class AmenityRepository : IAmenityRepository
    {
        private readonly AppDbContext _context;
        public AmenityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Amenity?> GetByIdAsync(string id)
        {
            return await _context.Amenities.FindAsync(id);
        }

        public async Task<List<Amenity>> GetAllAsync()
        {
            return await _context.Amenities.ToListAsync();
        }

        public async Task<(IEnumerable<Amenity>, int)> GetByFilterAsync(
            EntityFilter<Amenity> filter,
            EntitySort<Amenity> sort,
            EntityPage page)
        {
            var query = _context.Amenities.AsNoTracking();

            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var items = await query.Page(page).ToListAsync();

            return (items, totalCount);
        }

        public async Task AddAsync(Amenity amenity)
        {
            _context.Amenities.Add(amenity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Amenity amenity)
        {
            _context.Amenities.Update(amenity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var amenity = await _context.Amenities.FindAsync(id);
            if (amenity != null)
            {
                _context.Amenities.Remove(amenity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Amenities.AnyAsync(a => a.Id == id);
        }
    }
}
