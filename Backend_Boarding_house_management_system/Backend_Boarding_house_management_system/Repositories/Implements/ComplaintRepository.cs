using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Implements.Base;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Page;
using Plainquire.Sort;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class ComplaintRepository : Repository<Complaint, string>, IComplaintRepository
    {
        public ComplaintRepository(AppDbContext context) : base(context) { }

        private IQueryable<Complaint> GetDetailsQuery()
            => _dbSet.Include(c => c.Creator);

        public async Task<Complaint?> GetByIdWithDetailsAsync(string id)
            => await GetDetailsQuery().FirstOrDefaultAsync(c => c.Id == id);

        public async Task<(IEnumerable<Complaint> Items, int TotalCount)> GetByFilterWithDetailsAsync(
            EntityFilter<Complaint> filter,
            EntitySort<Complaint> sort,
            EntityPage page, string? landlordId = null)
        {
            page = EnsurePage(page);
            var query = GetDetailsQuery().AsNoTracking();

            if (!string.IsNullOrEmpty(landlordId))
            {
                var propertyIds = _context.Properties.Where(p => p.LandlordId == landlordId).Select(p => p.Id);
                var contractIds = _context.Contracts.Where(c => propertyIds.Contains(c.PropertyId)).Select(c => c.Id);
                var invoiceIds = _context.Invoices.Where(i => contractIds.Contains(i.ContractId)).Select(i => i.Id);
                
                query = query.Where(c => 
                    (c.RelatedType == ComplaintRelatedType.Property && propertyIds.Contains(c.RelatedId)) ||
                    (c.RelatedType == ComplaintRelatedType.Contract && contractIds.Contains(c.RelatedId)) ||
                    (c.RelatedType == ComplaintRelatedType.Invoice && invoiceIds.Contains(c.RelatedId)));
            }

            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var items = await query.Page(page).ToListAsync();
            return (items, totalCount);
        }
    }
}
