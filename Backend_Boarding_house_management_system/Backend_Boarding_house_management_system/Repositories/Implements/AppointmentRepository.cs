using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Implements.Base;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;
namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class AppointmentRepository : Repository<Appointment, string>, IAppointmentRepository
    {
        public AppointmentRepository(AppDbContext context) : base(context) { }

        protected override IQueryable<Appointment> GetQueryWithIncludes()
            => _dbSet
                .Include(a => a.Property)
                .Include(a => a.User);

        public override async Task<Appointment?> GetByIdAsync(string id)
            => await GetQueryWithIncludes()
                .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<(IEnumerable<Appointment> Items, int TotalCount)> GetAppointmentsWithLandlordFilterAsync(
            Plainquire.Filter.EntityFilter<Appointment> filter,
            Plainquire.Sort.EntitySort<Appointment> sort,
            Plainquire.Page.EntityPage page, string? landlordId)
        {
            page = EnsurePage(page);
            var query = GetQueryWithIncludes().AsNoTracking();

            if (!string.IsNullOrEmpty(landlordId))
            {
                query = query.Where(a => a.Property.LandlordId == landlordId);
            }

            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var items = await query.Page(page).ToListAsync();

            return (items, totalCount);
        }
    }
}
