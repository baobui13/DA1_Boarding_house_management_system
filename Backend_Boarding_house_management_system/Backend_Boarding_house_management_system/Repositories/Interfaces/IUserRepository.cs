using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces.Base;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User, string>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> UpdateAvatarAsync(string userId, string avatarUrl);
        Task<bool> BlockUserAsync(string userId, bool isBlocked);
    }
}