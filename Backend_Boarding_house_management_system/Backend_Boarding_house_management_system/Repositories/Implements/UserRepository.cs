using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Implements.Base;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class UserRepository : Repository<User, string>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
            => await _dbSet.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<bool> UpdateAvatarAsync(string userId, string avatarUrl)
        {
            var user = await _dbSet.FindAsync(userId);
            if (user == null) return false;
            user.AvatarUrl = avatarUrl;
            _dbSet.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> BlockUserAsync(string userId, bool isBlocked)
        {
            var user = await _dbSet.FindAsync(userId);
            if (user == null) return false;
            user.LockoutEnabled = isBlocked;
            user.LockoutEnd = isBlocked ? DateTimeOffset.MaxValue : null;
            _dbSet.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}