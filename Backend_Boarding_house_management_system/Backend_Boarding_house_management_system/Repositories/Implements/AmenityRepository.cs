using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.DTOs.Amenity.Requests;
using Backend_Boarding_house_management_system.DTOs.Base;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<PagedResponse<Amenity>> GetByFilterAsync(GetAmenitiesByFilterRequest request)
        {
            var query = _context.Amenities.AsQueryable();
            if (!string.IsNullOrEmpty(request.Name))
            {
                query = query.Where(a => a.Name.Contains(request.Name));
            }
            var total = await query.CountAsync();
            var items = await query.Skip((request.PageNumber - 1) * request.PageSize)
                                   .Take(request.PageSize)
                                   .ToListAsync();
            return new PagedResponse<Amenity>(items, total, request.PageNumber, request.PageSize);
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
