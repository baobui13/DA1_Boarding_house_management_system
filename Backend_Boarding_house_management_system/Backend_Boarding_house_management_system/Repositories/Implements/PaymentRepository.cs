using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Implements.Base;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class PaymentRepository : Repository<Payment, string>, IPaymentRepository
    {
        public PaymentRepository(AppDbContext context) : base(context) { }

        protected override IQueryable<Payment> GetQueryWithIncludes()
            => _dbSet.Include(p => p.Invoice);

        public override async Task<Payment?> GetByIdAsync(string id)
            => await GetQueryWithIncludes()
                .FirstOrDefaultAsync(p => p.Id == id);
    }
}
