using Microsoft.EntityFrameworkCore;

namespace Backend_Boarding_house_management_system.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //khai báo database
        // public DbSet<User> Users { get; set; }
    }
}
