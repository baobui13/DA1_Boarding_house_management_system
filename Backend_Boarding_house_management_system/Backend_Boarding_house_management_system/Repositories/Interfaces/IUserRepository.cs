using Backend_Boarding_house_management_system.Entities;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(string id);

        Task<User?> GetUserByEmailAsync(string email);

        Task<(IEnumerable<User>, int)> GetUsersByFilterAsync(string? role, string? fullName, string? address, DateTime? createdAfter, DateTime? createdBefore, string sortBy, bool isDescending, int pageNumber, int pageSize);

        Task<bool> UpdateUserAsync(User user);

        Task<bool> UpdateUserAvatarAsync(string userId, string avatarUrl);

        Task<bool> BlockUserAsync(string userId, bool isBlocked);

        Task<bool> DeleteUserAsync(string userId);
    }
}