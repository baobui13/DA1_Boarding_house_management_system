using Backend_Boarding_house_management_system.Entities;
using System.Threading.Tasks;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IPropertyImageRepository
    {
        Task AddImageAsync(PropertyImage image);
        Task<bool> SaveChangesAsync();
    }
}
