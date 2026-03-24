using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

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

        public async Task<(IEnumerable<Property>, int)> GetPropertiesByFilterAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page)
        {
            var query = _context.Properties.AsNoTracking();

            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var properties = await query.Page(page).ToListAsync();

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
