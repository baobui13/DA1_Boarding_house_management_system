using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly AppDbContext _context;
        public PropertyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Property?> GetPropertyByIdAsync(string id)
        {
            return await _context.Properties.FindAsync(id);
        }

        public async Task<(IEnumerable<Property>, int)> GetPropertiesByFilterAsync(string? landlordId, string? areaId, string? propertyName, string? address, decimal? minPrice, decimal? maxPrice, string? status, decimal? minSize, decimal? maxSize, DateTime? createdAfter, DateTime? createdBefore, string sortBy, bool isDescending, int pageNumber, int pageSize)
        {
            var query = _context.Properties.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(landlordId))
                query = query.Where(p => p.LandlordId == landlordId);

            if (!string.IsNullOrEmpty(areaId))
                query = query.Where(p => p.AreaId == areaId);

            if (!string.IsNullOrEmpty(propertyName))
                query = query.Where(p => p.PropertyName.Contains(propertyName));

            if (!string.IsNullOrEmpty(address))

                query = query.Where(p => p.Address != null && p.Address.Contains(address));
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (minSize.HasValue)
                query = query.Where(p => p.Size >= minSize.Value);

            if (maxSize.HasValue)
                query = query.Where(p => p.Size <= maxSize.Value);

            if (createdAfter.HasValue)
                query = query.Where(p => p.CreatedAt >= createdAfter.Value);

            if (createdBefore.HasValue)
                query = query.Where(p => p.CreatedAt <= createdBefore.Value);

            query = query.OrderBy($"{sortBy} {(isDescending ? "descending" : "ascending")}");

            var totalCount = await query.CountAsync();
            var properties = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return (properties, totalCount);
        }

        public async Task<Property> CreatePropertyAsync(Property property)
        {
            _context.Properties.Add(property);
            var result = await _context.SaveChangesAsync() > 0;
            return result ? property : null!;
        }

        public async Task<bool> UpdatePropertyAsync(Property property)
        {
            _context.Properties.Update(property);
            var result = await _context.SaveChangesAsync() > 0;
            return result;
        }

        public async Task<bool> DeletePropertyAsync(string propertyId)
        {
            var property = await _context.Properties.FindAsync(propertyId);
            if (property == null)
                return false;
            _context.Properties.Remove(property);
            var result = await _context.SaveChangesAsync() > 0;
            return result;
        }
    }
}
