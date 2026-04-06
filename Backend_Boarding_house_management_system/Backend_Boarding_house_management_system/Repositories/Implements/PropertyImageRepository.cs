using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Implements.Base;
using Backend_Boarding_house_management_system.Repositories.Interfaces;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class PropertyImageRepository : Repository<PropertyImage, string>, IPropertyImageRepository
    {
        public PropertyImageRepository(AppDbContext context) : base(context) { }
    }
}
