using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Implements.Base;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class RoomAmenityRepository : Repository<RoomAmenity, string>, IRoomAmenityRepository
    {
        public RoomAmenityRepository(AppDbContext context) : base(context) { }

        protected override IQueryable<RoomAmenity> GetQueryWithIncludes()
            => _dbSet
                .Include(r => r.Property)
                .Include(r => r.Amenity);

        public override async Task<RoomAmenity?> GetByIdAsync(string id)
            => await GetQueryWithIncludes()
                .FirstOrDefaultAsync(r => r.Id == id);

        public async Task<bool> ExistsForPropertyAndAmenityAsync(string propertyId, string amenityId)
            => await _dbSet.AnyAsync(r => r.PropertyId == propertyId && r.AmenityId == amenityId);
    }
}
