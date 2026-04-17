using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Implements.Base;
using Backend_Boarding_house_management_system.Repositories.Interfaces;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class AmenityRepository : Repository<Amenity, string>, IAmenityRepository
    {
        public AmenityRepository(AppDbContext context) : base(context) { }
    }
}
