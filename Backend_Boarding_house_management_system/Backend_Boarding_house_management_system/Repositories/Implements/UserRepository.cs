using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<(IEnumerable<User>, int)> GetUsersByFilterAsync(string? role, string? fullName, string? address, DateTime? createdAfter, DateTime? createdBefore, string sortBy, bool isDescending, int pageNumber, int pageSize)
        {
            var query = _context.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.Role == role);

            if (!string.IsNullOrEmpty(fullName))
                query = query.Where(u => u.FullName.Contains(fullName));

            if (!string.IsNullOrEmpty(address))
                query = query.Where(u => u.Address != null && u.Address.Contains(address));

            if (createdAfter.HasValue)
                query = query.Where(u => u.CreatedAt >= createdAfter.Value);

            if (createdBefore.HasValue)
                query = query.Where(u => u.CreatedAt <= createdBefore.Value);

            // Sorting
            query = query.OrderBy($"{sortBy} {(isDescending ? "descending" : "ascending")}");

            // Pagination
            var totalCount = await query.CountAsync();
            var users = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return (users, totalCount);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateUserAvatarAsync(string userId, string avatarUrl)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.AvatarUrl = avatarUrl;
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> BlockUserAsync(string userId, bool isBlocked)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.LockoutEnabled = isBlocked;
            user.LockoutEnd = isBlocked ? DateTimeOffset.MaxValue : null;
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            _context.Users.Remove(user);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}