using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Google;
using System.Threading.Tasks;
using Backend_Boarding_house_management_system.Data;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class PropertyImageRepository : IPropertyImageRepository
    {
        private readonly AppDbContext _context;

        public PropertyImageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddImageAsync(PropertyImage image)
        {
            await _context.PropertyImages.AddAsync(image);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
