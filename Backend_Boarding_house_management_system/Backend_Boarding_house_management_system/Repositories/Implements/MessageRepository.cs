using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Implements.Base;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class MessageRepository : Repository<Message, string>, IMessageRepository
    {
        public MessageRepository(AppDbContext context) : base(context) { }

        protected override IQueryable<Message> GetQueryWithIncludes()
            => _dbSet
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Property)
                .Include(m => m.Contract);

        public override async Task<Message?> GetByIdAsync(string id)
            => await GetQueryWithIncludes()
                .FirstOrDefaultAsync(m => m.Id == id);
    }
}
