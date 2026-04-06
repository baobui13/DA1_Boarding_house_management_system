using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Implements.Base;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class SearchHistoryRepository : Repository<SearchHistory, string>, ISearchHistoryRepository
    {
        public SearchHistoryRepository(AppDbContext context) : base(context) { }

        protected override IQueryable<SearchHistory> GetQueryWithIncludes()
            => _dbSet.Include(sh => sh.User);

        public override async Task<SearchHistory?> GetByIdAsync(string id)
            => await GetQueryWithIncludes()
                .FirstOrDefaultAsync(sh => sh.Id == id);
    }
}
