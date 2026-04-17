using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Implements.Base;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class ViewHistoryRepository : Repository<ViewHistory, string>, IViewHistoryRepository
    {
        public ViewHistoryRepository(AppDbContext context) : base(context) { }

        protected override IQueryable<ViewHistory> GetQueryWithIncludes()
            => _dbSet
                .Include(vh => vh.User)
                .Include(vh => vh.Property);

        public override async Task<ViewHistory?> GetByIdAsync(string id)
            => await GetQueryWithIncludes()
                .FirstOrDefaultAsync(vh => vh.Id == id);
    }
}
