using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Implements.Base;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Page;
using Plainquire.Sort;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class PropertyRepository : Repository<Property, string>, IPropertyRepository
    {
        public PropertyRepository(AppDbContext context) : base(context) { }

        private IQueryable<Property> GetDetailsQuery()
            => _dbSet
                .Include(p => p.Landlord)
                .Include(p => p.Area)
                .Include(p => p.PropertyImages)
                .Include(p => p.RoomAmenities)
                    .ThenInclude(ra => ra.Amenity)
                .Include(p => p.Contracts)
                .Include(p => p.Appointments);

        public async Task<Property?> GetByIdWithDetailsAsync(string id)
            => await GetDetailsQuery().FirstOrDefaultAsync(p => p.Id == id);

        public async Task<(IEnumerable<Property> Items, int TotalCount)> GetByFilterWithDetailsAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page)
        {
            var query = GetDetailsQuery().AsNoTracking();
            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var items = await query.Page(page).ToListAsync();
            return (items, totalCount);
        }
    }
}
