using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppDbContext _context;

        public AppointmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(string id)
        {
            return await _context.Appointments
                .Include(a => a.Property)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<(IEnumerable<Appointment>, int)> GetAppointmentsByFilterAsync(
            EntityFilter<Appointment> filter,
            EntitySort<Appointment> sort,
            EntityPage page)
        {
            var query = _context.Appointments
                .Include(a => a.Property)
                .Include(a => a.User)
                .AsNoTracking();

            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var appointments = await query.Page(page).ToListAsync();

            return (appointments, totalCount);
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            var result = await _context.SaveChangesAsync() > 0;
            return result ? appointment : null!;
        }

        public async Task<bool> UpdateAppointmentAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAppointmentAsync(string id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return false;

            _context.Appointments.Remove(appointment);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
