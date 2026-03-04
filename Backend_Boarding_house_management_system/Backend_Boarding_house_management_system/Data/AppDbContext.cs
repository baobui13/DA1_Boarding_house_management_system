using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Backend_Boarding_house_management_system.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //khai báo database

        // IdentityDbContext có sẵn bảng User
        public DbSet<Area> Areas { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<RoomAmenity> RoomAmenities { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SearchHistory> SearchHistories { get; set; }
        public DbSet<ViewHistory> ViewHistories { get; set; }
        public DbSet<TenantDocument> TenantDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // cấu hình enum và quan hệ của Entities
            modelBuilder.ConfigureRelationshipsAndEnums();
        }
    }
}
