using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.DTOs.RoomAmenity.Requests;
using Backend_Boarding_house_management_system.DTOs.Base;
using Microsoft.EntityFrameworkCore;

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

        public async Task<PagedResponse<RoomAmenity>> GetByFilterAsync(GetRoomAmenitiesByFilterRequest request)
        {
            var query = _context.RoomAmenities
                .Include(r => r.Amenity)
                .Include(r => r.Room)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.RoomId))
            {
                query = query.Where(r => r.RoomId == request.RoomId);
            }

            if (!string.IsNullOrEmpty(request.AmenityId))
            {
                query = query.Where(r => r.AmenityId == request.AmenityId);
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(r => r.Status == request.Status);
            }

            var total = await query.CountAsync();
            var items = await query.Skip((request.PageNumber - 1) * request.PageSize)
                                   .Take(request.PageSize)
                                   .ToListAsync();

            return new PagedResponse<RoomAmenity>(items, total, request.PageNumber, request.PageSize);
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
