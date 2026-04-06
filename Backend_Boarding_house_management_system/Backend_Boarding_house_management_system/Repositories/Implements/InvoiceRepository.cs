using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Implements.Base;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class InvoiceRepository : Repository<Invoice, string>, IInvoiceRepository
    {
        public InvoiceRepository(AppDbContext context) : base(context) { }

        protected override IQueryable<Invoice> GetQueryWithIncludes()
            => _dbSet.Include(i => i.Contract);

        public override async Task<Invoice?> GetByIdAsync(string id)
            => await GetQueryWithIncludes()
                .FirstOrDefaultAsync(i => i.Id == id);
    }
}
