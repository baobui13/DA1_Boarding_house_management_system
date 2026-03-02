using Microsoft.EntityFrameworkCore;
using Backend_Boarding_house_management_system.Entities;

namespace Backend_Boarding_house_management_system.Extensions
{
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Cấu hình toàn bộ quan hệ, DeleteBehavior, index, enum conversion cho dự án
        /// </summary>
        public static ModelBuilder ConfigureRelationshipsAndEnums(this ModelBuilder modelBuilder)
        {
            // ───────────────────────────────────────────────
            // 1. Cấu hình tất cả enum lưu dạng string (nvarchar/varchar)
            // ───────────────────────────────────────────────
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Property>()
                .Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<RoomAmenity>()
                .Property(ra => ra.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Contract>()
                .Property(c => c.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Method)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Notification>()
                .Property(n => n.Type)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<TenantDocument>()
                .Property(td => td.DocumentType)
                .HasConversion<string>()
                .HasMaxLength(50);

            // ───────────────────────────────────────────────
            // 2. Cấu hình các quan hệ quan trọng + DeleteBehavior
            // ───────────────────────────────────────────────

            // User (Landlord) → Area
            modelBuilder.Entity<Area>()
                .HasOne(a => a.Landlord)
                .WithMany(u => u.Areas)
                .HasForeignKey(a => a.LandlordId)
                .OnDelete(DeleteBehavior.Restrict);     // Không cho xóa Landlord nếu còn khu trọ

            // User → Property (phòng thuộc chủ trọ)
            modelBuilder.Entity<Property>()
                .HasOne(p => p.Landlord)
                .WithMany(u => u.Properties)
                .HasForeignKey(p => p.LandlordId)
                .OnDelete(DeleteBehavior.Restrict);     // Bảo vệ phòng không bị xóa theo user

            // Area → Property (phòng thuộc khu)
            modelBuilder.Entity<Property>()
                .HasOne(p => p.Area)
                .WithMany(a => a.Properties)
                .HasForeignKey(p => p.AreaId)
                .IsRequired(false)                      // optional
                .OnDelete(DeleteBehavior.SetNull);      // Xóa khu → phòng vẫn tồn tại, AreaId = null

            // Property → Contract
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Room)
                .WithMany(p => p.Contracts)
                .HasForeignKey(c => c.RoomId)
                .OnDelete(DeleteBehavior.Restrict);     // Không xóa phòng nếu đang có hợp đồng

            // Contract → Invoice
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Contract)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.ContractId)
                .OnDelete(DeleteBehavior.Cascade);      // Xóa hợp đồng → xóa hóa đơn liên quan

            // Property → RoomAmenity (junction)
            modelBuilder.Entity<RoomAmenity>()
                .HasOne(ra => ra.Room)
                .WithMany(p => p.RoomAmenities)
                .HasForeignKey(ra => ra.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RoomAmenity>()
                .HasOne(ra => ra.Amenity)
                .WithMany(a => a.RoomAmenities)
                .HasForeignKey(ra => ra.AmenityId)
                .OnDelete(DeleteBehavior.Cascade);      // Xóa tiện ích -> xóa liên kết giữa tiện ích và các phòng

            // Property → PropertyImage
            modelBuilder.Entity<PropertyImage>()
                .HasOne(pi => pi.Property)
                .WithMany(p => p.PropertyImages)
                .HasForeignKey(pi => pi.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);      // Xóa phòng -> xóa luôn ảnh

            // Invoice → Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // ───────────────────────────────────────────────
            // 3. Index hữu ích cho tìm kiếm & báo cáo
            // ───────────────────────────────────────────────

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email_Unique");

            modelBuilder.Entity<Property>()
                .HasIndex(p => p.Status)
                .HasDatabaseName("IX_Properties_Status");

            modelBuilder.Entity<Contract>()
                .HasIndex(c => c.Status)
                .HasDatabaseName("IX_Contracts_Status");

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => new { i.Status, i.DueDate })
                .HasDatabaseName("IX_Invoices_Status_DueDate");

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.AppointmentDateTime);


            return modelBuilder;
        }
    }
}
