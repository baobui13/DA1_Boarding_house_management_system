using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Google;
using System.Threading.Tasks;
using Backend_Boarding_house_management_system.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class PropertyImageRepository : IPropertyImageRepository
    {
        private readonly AppDbContext _context;

        public PropertyImageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PropertyImage?> GetPropertyImageByIdAsync(string id)
        {
            return await _context.PropertyImages.FindAsync(id);
        }

        public async Task<(IEnumerable<PropertyImage>, int)> GetPropertyImagesByFilterAsync(string? propertyId, bool? isPrimary, string sortBy, bool isDescending, int pageNumber, int pageSize)
        {
            var query = _context.PropertyImages.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(propertyId))
                query = query.Where(i => i.PropertyId == propertyId);

            if (isPrimary.HasValue)
                query = query.Where(i => i.IsPrimary == isPrimary.Value);

            var totalCount = await query.CountAsync();
            query = query.OrderBy(sortBy + (isDescending ? " descending" : ""));
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return (await query.ToListAsync(), totalCount);
        }

        public async Task<PropertyImage> CreatePropertyImageAsync(PropertyImage propertyImage)
        {
            _context.PropertyImages.Add(propertyImage);
            await _context.SaveChangesAsync();
            return propertyImage;
        }

        public async Task<bool> UpdatePropertyImageAsync(PropertyImage propertyImage)
        {
            _context.PropertyImages.Update(propertyImage);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeletePropertyImageAsync(string propertyImageId)
        {
            var image = await _context.PropertyImages.FindAsync(propertyImageId);
            if (image == null) return false;
            _context.PropertyImages.Remove(image);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
