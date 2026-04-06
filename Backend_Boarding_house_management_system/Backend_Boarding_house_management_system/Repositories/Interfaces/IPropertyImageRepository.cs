using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces.Base;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IPropertyImageRepository : IRepository<PropertyImage, string>
    {
        // Tất cả CRUD chung đã được kế thừa từ IRepository.
    }
}
