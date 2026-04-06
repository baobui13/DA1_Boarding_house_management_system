using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Implements.Base;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class TenantDocumentRepository : Repository<TenantDocument, string>, ITenantDocumentRepository
    {
        public TenantDocumentRepository(AppDbContext context) : base(context) { }

        protected override IQueryable<TenantDocument> GetQueryWithIncludes()
            => _dbSet.Include(td => td.Tenant);

        public override async Task<TenantDocument?> GetByIdAsync(string id)
            => await GetQueryWithIncludes()
                .FirstOrDefaultAsync(td => td.Id == id);
    }
}
