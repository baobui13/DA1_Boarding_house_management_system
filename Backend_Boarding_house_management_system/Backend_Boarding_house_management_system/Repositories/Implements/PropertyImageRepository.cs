using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Data;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

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

        public async Task<(IEnumerable<PropertyImage>, int)> GetPropertyImagesByFilterAsync(
            EntityFilter<PropertyImage> filter,
            EntitySort<PropertyImage> sort,
            EntityPage page)
        {
            var query = _context.PropertyImages.AsNoTracking();

            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var images = await query.Page(page).ToListAsync();

            return (images, totalCount);
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
