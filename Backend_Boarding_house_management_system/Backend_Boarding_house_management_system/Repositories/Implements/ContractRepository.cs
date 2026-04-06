using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class ContractRepository : IContractRepository
    {
        private readonly AppDbContext _context;
        public ContractRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Contract?> GetByIdAsync(string id)
        {
            return await _context.Contracts
                .Include(c => c.Property)
                .Include(c => c.Tenant)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<(IEnumerable<Contract>, int)> GetByFilterAsync(
            EntityFilter<Contract> filter,
            EntitySort<Contract> sort,
            EntityPage page)
        {
            var query = _context.Contracts
                .Include(c => c.Property)
                .Include(c => c.Tenant)
                .AsNoTracking();

            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var items = await query.Page(page).ToListAsync();

            return (items, totalCount);
        }

        public async Task AddAsync(Contract entity)
        {
            _context.Contracts.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Contract entity)
        {
            _context.Contracts.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _context.Contracts.FindAsync(id);
            if (entity != null)
            {
                _context.Contracts.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Contracts.AnyAsync(c => c.Id == id);
        }
    }
}
