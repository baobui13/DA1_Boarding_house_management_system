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

            modelBuilder.Entity<Rating>()
                .Property(r => r.AIAttitude)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Complaint>()
                .Property(c => c.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Complaint>()
                .Property(c => c.RelatedType)
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
                .OnDelete(DeleteBehavior.Restrict);

            // User → Property (phòng thuộc chủ trọ)
            modelBuilder.Entity<Property>()
                .HasOne(p => p.Landlord)
                .WithMany(u => u.Properties)
                .HasForeignKey(p => p.LandlordId)
                .OnDelete(DeleteBehavior.Restrict);

            // Area → Property (phòng thuộc khu)
            modelBuilder.Entity<Property>()
                .HasOne(p => p.Area)
                .WithMany(a => a.Properties)
                .HasForeignKey(p => p.AreaId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Property → Contract
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Property)
                .WithMany(p => p.Contracts)
                .HasForeignKey(c => c.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            // User (Tenant) → Contract
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Tenant)
                .WithMany(u => u.ContractsAsTenant)
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Contract → Invoice
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Contract)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            // Invoice → Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Property → RoomAmenity (junction)
            modelBuilder.Entity<RoomAmenity>()
                .HasOne(ra => ra.Property)
                .WithMany(p => p.RoomAmenities)
                .HasForeignKey(ra => ra.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Amenity -> RoomAmenity
            modelBuilder.Entity<RoomAmenity>()
                .HasOne(ra => ra.Amenity)
                .WithMany(a => a.RoomAmenities)
                .HasForeignKey(ra => ra.AmenityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Property → PropertyImage
            modelBuilder.Entity<PropertyImage>()
                .HasOne(pi => pi.Property)
                .WithMany(p => p.PropertyImages)
                .HasForeignKey(pi => pi.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // User → Message
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.MessagesSent)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.MessagesReceived)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Property → Message
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Property)
                .WithMany(p => p.Messages)
                .HasForeignKey(m => m.PropertyId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Contract → Message
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Contract)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ContractId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // User → Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Property → Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Property)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // User → Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User → SearchHistory
            modelBuilder.Entity<SearchHistory>()
                .HasOne(sh => sh.User)
                .WithMany(u => u.SearchHistories)
                .HasForeignKey(sh => sh.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User → ViewHistory
            modelBuilder.Entity<ViewHistory>()
                .HasOne(vh => vh.User)
                .WithMany(u => u.ViewHistories)
                .HasForeignKey(vh => vh.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Property → ViewHistory
            modelBuilder.Entity<ViewHistory>()
                .HasOne(vh => vh.Property)
                .WithMany(p => p.ViewHistories)
                .HasForeignKey(vh => vh.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // User → TenantDocument
            modelBuilder.Entity<TenantDocument>()
                .HasOne(td => td.Tenant)
                .WithMany(u => u.TenantDocuments)
                .HasForeignKey(td => td.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // User → Rating
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Tenant)
                .WithMany(u => u.Ratings)
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Property → Rating
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Property)
                .WithMany(p => p.Ratings)
                .HasForeignKey(r => r.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // User → Complaint
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Creator)
                .WithMany(u => u.Complaints)
                .HasForeignKey(c => c.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // User → RefreshToken
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ───────────────────────────────────────────────
            // 3. Index hữu ích cho tìm kiếm & báo cáo
            // ───────────────────────────────────────────────

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users"); // Đặt tên bảng là Users
            });

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
