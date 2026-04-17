using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Implements.Base;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class ContractRepository : Repository<Contract, string>, IContractRepository
    {
        public ContractRepository(AppDbContext context) : base(context) { }

        protected override IQueryable<Contract> GetQueryWithIncludes()
            => _dbSet
                .Include(c => c.Property)
                .Include(c => c.Tenant);

        public override async Task<Contract?> GetByIdAsync(string id)
            => await GetQueryWithIncludes()
                .FirstOrDefaultAsync(c => c.Id == id);
    }
}
