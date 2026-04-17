using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Repositories.Interfaces.Base;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Page;
using Plainquire.Sort;

namespace Backend_Boarding_house_management_system.Repositories.Implements.Base
{
    public class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        protected virtual IQueryable<TEntity> GetQueryWithIncludes()
            => _dbSet;

        public virtual async Task<TEntity?> GetByIdAsync(TKey id)
            => await _dbSet.FindAsync(id);

        public virtual async Task<List<TEntity>> GetAllAsync()
            => await GetQueryWithIncludes()
                .AsNoTracking()
                .ToListAsync();

        public virtual async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetByFilterAsync(
            EntityFilter<TEntity> filter,
            EntitySort<TEntity> sort,
            EntityPage page)
        {
            var query = GetQueryWithIncludes().AsNoTracking();

            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var items = await query.Page(page).ToListAsync();

            return (items, totalCount);
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(TKey id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
            {
                return;
            }

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task<bool> ExistsAsync(TKey id)
            => await GetByIdAsync(id) != null;
    }
}
