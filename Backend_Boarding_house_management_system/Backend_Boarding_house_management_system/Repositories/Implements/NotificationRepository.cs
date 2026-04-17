using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Implements.Base;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class NotificationRepository : Repository<Notification, string>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context) { }

        protected override IQueryable<Notification> GetQueryWithIncludes()
            => _dbSet.Include(n => n.User);

        public override async Task<Notification?> GetByIdAsync(string id)
            => await GetQueryWithIncludes()
                .FirstOrDefaultAsync(n => n.Id == id);
    }
}
