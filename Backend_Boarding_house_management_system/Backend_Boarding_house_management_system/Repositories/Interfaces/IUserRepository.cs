using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(string id);

        Task<User?> GetUserByEmailAsync(string email);

        Task<(IEnumerable<User>, int)> GetUsersByFilterAsync(
            EntityFilter<User> filter,
            EntitySort<User> sort,
            EntityPage page);

        Task<bool> UpdateUserAsync(User user);

        Task<bool> UpdateUserAvatarAsync(string userId, string avatarUrl);

        Task<bool> BlockUserAsync(string userId, bool isBlocked);

        Task<bool> DeleteUserAsync(string userId);
    }
}