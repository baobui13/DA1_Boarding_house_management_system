using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _context;
        public MessageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Message?> GetByIdAsync(string id)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Property)
                .Include(m => m.Contract)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(IEnumerable<Message>, int)> GetByFilterAsync(
            EntityFilter<Message> filter,
            EntitySort<Message> sort,
            EntityPage page)
        {
            var query = _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Property)
                .Include(m => m.Contract)
                .AsNoTracking();

            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var items = await query.Page(page).ToListAsync();

            return (items, totalCount);
        }

        public async Task AddAsync(Message entity)
        {
            _context.Messages.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Message entity)
        {
            _context.Messages.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _context.Messages.FindAsync(id);
            if (entity != null)
            {
                _context.Messages.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Messages.AnyAsync(m => m.Id == id);
        }
    }
}
