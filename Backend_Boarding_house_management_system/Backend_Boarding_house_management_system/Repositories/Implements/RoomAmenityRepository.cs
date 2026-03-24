using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class RoomAmenityRepository : IRoomAmenityRepository
    {
        private readonly AppDbContext _context;
        public RoomAmenityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RoomAmenity?> GetByIdAsync(string id)
        {
            return await _context.RoomAmenities
                .Include(r => r.Amenity)
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<(IEnumerable<RoomAmenity>, int)> GetByFilterAsync(
            EntityFilter<RoomAmenity> filter,
            EntitySort<RoomAmenity> sort,
            EntityPage page)
        {
            var query = _context.RoomAmenities
                .Include(r => r.Amenity)
                .Include(r => r.Room)
                .AsNoTracking();

            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var items = await query.Page(page).ToListAsync();

            return (items, totalCount);
        }

        public async Task AddAsync(RoomAmenity entity)
        {
            _context.RoomAmenities.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RoomAmenity entity)
        {
            _context.RoomAmenities.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _context.RoomAmenities.FindAsync(id);
            if (entity != null)
            {
                _context.RoomAmenities.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.RoomAmenities.AnyAsync(r => r.Id == id);
        }

        public async Task<bool> ExistsForRoomAndAmenityAsync(string roomId, string amenityId)
        {
            return await _context.RoomAmenities.AnyAsync(r => r.RoomId == roomId && r.AmenityId == amenityId);
        }
    }
}
