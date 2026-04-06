using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Implements.Base;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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
    }
}
