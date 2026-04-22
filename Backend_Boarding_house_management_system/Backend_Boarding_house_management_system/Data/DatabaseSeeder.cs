using Backend_Boarding_house_management_system.Entities;
using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Boarding_house_management_system.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedDataAsync(AppDbContext context)
        {
            // Kiểm tra nếu hệ thống đã có dữ liệu thì không seed lại
            if (await context.Users.AnyAsync()) return;

            var hasher = new PasswordHasher<User>();
            var rnd = new Random();

            var users = SeedUsers(context, hasher);
            var landlords = users.Where(u => u.Role == "Landlord").ToList();
            var tenants = users.Where(u => u.Role == "Tenant").ToList();

            var areas = SeedAreas(context, landlords);
            var properties = SeedProperties(context, landlords, areas);
            var amenities = SeedAmenities(context);
            
            SeedRoomAmenities(context, properties, amenities, rnd);
            
            var contracts = SeedContracts(context, properties, tenants);
            var invoices = SeedInvoices(context, contracts, properties, rnd);
            SeedPayments(context, invoices, rnd);
            
            SeedAppointments(context, properties, tenants);
            SeedRatings(context, properties, tenants);
            SeedComplaints(context, tenants);
            SeedMessages(context, users);
            SeedNotifications(context, users);

            // Lưu tất cả thay đổi vào database
            await context.SaveChangesAsync();
        }

        private static List<User> SeedUsers(AppDbContext context, PasswordHasher<User> hasher)
        {
            var users = new List<User>();
            
            var admin = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "admin@test.com",
                Email = "admin@test.com",
                NormalizedEmail = "ADMIN@TEST.COM",
                NormalizedUserName = "ADMIN@TEST.COM",
                FullName = "System Admin",
                CCCD = "000000000000",
                Role = "Admin",
                EmailConfirmed = true
            };
            admin.PasswordHash = hasher.HashPassword(admin, "Password123!");
            users.Add(admin);

            var landlords = new Faker<User>("vi")
                .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.UserName, f => f.Internet.Email())
                .RuleFor(u => u.Email, (f, u) => u.UserName)
                .RuleFor(u => u.NormalizedEmail, (f, u) => u.Email?.ToUpper())
                .RuleFor(u => u.NormalizedUserName, (f, u) => u.Email?.ToUpper())
                .RuleFor(u => u.FullName, f => f.Name.FullName())
                .RuleFor(u => u.CCCD, f => f.Random.Replace("0791########"))
                .RuleFor(u => u.Address, f => f.Address.FullAddress())
                .RuleFor(u => u.Role, "Landlord")
                .RuleFor(u => u.EmailConfirmed, true)
                .RuleFor(u => u.ReputationScore, f => f.Random.Int(50, 100))
                .Generate(15);
            foreach (var l in landlords) l.PasswordHash = hasher.HashPassword(l, "Password123!");
            users.AddRange(landlords);

            var tenants = new Faker<User>("vi")
                .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.UserName, f => f.Internet.Email())
                .RuleFor(u => u.Email, (f, u) => u.UserName)
                .RuleFor(u => u.NormalizedEmail, (f, u) => u.Email?.ToUpper())
                .RuleFor(u => u.NormalizedUserName, (f, u) => u.Email?.ToUpper())
                .RuleFor(u => u.FullName, f => f.Name.FullName())
                .RuleFor(u => u.CCCD, f => f.Random.Replace("0791########"))
                .RuleFor(u => u.Address, f => f.Address.FullAddress())
                .RuleFor(u => u.Role, "Tenant")
                .RuleFor(u => u.EmailConfirmed, true)
                .Generate(35);
            foreach (var t in tenants) t.PasswordHash = hasher.HashPassword(t, "Password123!");
            users.AddRange(tenants);

            context.Users.AddRange(users);
            return users;
        }

        private static List<Area> SeedAreas(AppDbContext context, List<User> landlords)
        {
            var areas = new Faker<Area>("vi")
                .RuleFor(a => a.Id, f => Guid.NewGuid().ToString())
                .RuleFor(a => a.Name, f => "Khu trọ " + f.Address.StreetName())
                .RuleFor(a => a.Address, f => f.Address.FullAddress())
                .RuleFor(a => a.LandlordId, f => f.PickRandom(landlords).Id)
                .RuleFor(a => a.RoomCount, f => f.Random.Int(5, 20))
                .RuleFor(a => a.Description, f => f.Lorem.Sentence())
                .Generate(15);
                
            context.Areas.AddRange(areas);
            return areas;
        }

        private static List<Property> SeedProperties(AppDbContext context, List<User> landlords, List<Area> areas)
        {
            var properties = new Faker<Property>("vi")
                .RuleFor(p => p.Id, f => Guid.NewGuid().ToString())
                .RuleFor(p => p.LandlordId, f => f.PickRandom(landlords).Id) 
                .RuleFor(p => p.AreaId, f => f.PickRandom(areas).Id)
                .RuleFor(p => p.PropertyName, f => "Phòng " + f.Random.Int(100, 999))
                .RuleFor(p => p.Size, f => f.Random.Decimal(15, 40))
                .RuleFor(p => p.Price, f => f.Random.Decimal(1000000, 5000000))
                .RuleFor(p => p.ElectricPrice, f => f.Random.Decimal(3000, 5000))
                .RuleFor(p => p.WaterPrice, f => f.Random.Decimal(10000, 30000))
                .RuleFor(p => p.Status, "Available") 
                .RuleFor(p => p.Address, f => f.Address.FullAddress())
                .RuleFor(p => p.Description, f => f.Lorem.Paragraph())
                .Generate(50);
            
            foreach(var p in properties) {
               var area = areas.First(a => a.Id == p.AreaId);
               p.LandlordId = area.LandlordId; 
            }
            context.Properties.AddRange(properties);
            return properties;
        }

        private static List<Amenity> SeedAmenities(AppDbContext context)
        {
            var amenityNames = new[] { "Wifi", "Điều hòa", "Tủ lạnh", "Máy giặt", "Nóng lạnh", "Giường", "Tủ quần áo", "Chức năng an ninh 24/7", "Thang máy" };
            var amenities = amenityNames.Select(n => new Amenity
            {
                Id = Guid.NewGuid().ToString(),
                Name = n,
                Description = "Tiện ích " + n
            }).ToList();
            context.Amenities.AddRange(amenities);
            return amenities;
        }

        private static void SeedRoomAmenities(AppDbContext context, List<Property> properties, List<Amenity> amenities, Random rnd)
        {
            var roomAmenities = new List<RoomAmenity>();
            foreach (var p in properties)
            {
                var pickedAmenities = amenities.OrderBy(x => rnd.Next()).Take(rnd.Next(3, 6)).ToList();
                foreach (var a in pickedAmenities)
                {
                    roomAmenities.Add(new RoomAmenity
                    {
                        Id = Guid.NewGuid().ToString(),
                        PropertyId = p.Id,
                        AmenityId = a.Id,
                        Status = rnd.Next(0, 10) > 1 ? "Working" : "Broken"
                    });
                }
            }
            context.RoomAmenities.AddRange(roomAmenities);
        }

        private static List<Contract> SeedContracts(AppDbContext context, List<Property> properties, List<User> tenants)
        {
            var contractStatuses = new[] { "Active", "Expired", "Terminated", "Draft" };
            var contracts = new Faker<Contract>("vi")
                .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
                .RuleFor(c => c.PropertyId, f => f.PickRandom(properties).Id)
                .RuleFor(c => c.TenantId, f => f.PickRandom(tenants).Id)
                .RuleFor(c => c.StartDate, f => f.Date.Past(1))
                .RuleFor(c => c.EndDate, (f, c) => c.StartDate.AddMonths(f.Random.Int(6, 12)))
                .RuleFor(c => c.Deposit, f => f.Random.Decimal(1000000, 5000000))
                .RuleFor(c => c.Terms, f => f.Lorem.Paragraph())
                .RuleFor(c => c.Status, f => f.PickRandom(contractStatuses))
                .Generate(25);

            foreach (var c in contracts.Where(x => x.Status == "Active"))
            {
                var p = properties.First(px => px.Id == c.PropertyId);
                p.Status = "Rented"; 
            }
            context.Contracts.AddRange(contracts);
            return contracts;
        }

        private static List<Invoice> SeedInvoices(AppDbContext context, List<Contract> contracts, List<Property> properties, Random rnd)
        {
            var invoices = new List<Invoice>();
            foreach (var c in contracts)
            {
                var invCount = rnd.Next(1, 4);
                for (int i = 0; i < invCount; i++)
                {
                    invoices.Add(new Invoice
                    {
                        Id = Guid.NewGuid().ToString(),
                        ContractId = c.Id,
                        Period = c.StartDate.AddMonths(i),
                        RentAmount = properties.First(p => p.Id == c.PropertyId).Price,
                        ElectricityCost = rnd.Next(50000, 300000),
                        WaterCost = rnd.Next(30000, 100000),
                        Total = properties.First(p => p.Id == c.PropertyId).Price + rnd.Next(80000, 400000),
                        Status = rnd.Next(0, 3) == 0 ? "Pending" : "Paid",
                        DueDate = c.StartDate.AddMonths(i).AddDays(5)
                    });
                }
            }
            context.Invoices.AddRange(invoices);
            return invoices;
        }

        private static void SeedPayments(AppDbContext context, List<Invoice> invoices, Random rnd)
        {
            var payments = new List<Payment>();
            foreach (var inv in invoices.Where(x => x.Status == "Paid"))
            {
                payments.Add(new Payment
                {
                    Id = Guid.NewGuid().ToString(),
                    InvoiceId = inv.Id,
                    Amount = inv.Total,
                    PaymentDate = inv.DueDate.AddDays(-rnd.Next(1, 5)),
                    Method = rnd.Next(0, 2) == 0 ? "BankTransfer" : "Cash"
                });
            }
            foreach (var inv in invoices.Where(x => x.Status == "Pending").Take(5))
            {
                inv.Status = "Partial";
                payments.Add(new Payment
                {
                    Id = Guid.NewGuid().ToString(),
                    InvoiceId = inv.Id,
                    Amount = inv.Total / 2,
                    PaymentDate = inv.DueDate.AddDays(-1),
                    Method = "Online"
                });
            }
            context.Payments.AddRange(payments);
        }

        private static void SeedAppointments(AppDbContext context, List<Property> properties, List<User> tenants)
        {
            var appStatuses = new[] { "Pending", "Confirmed", "Rejected", "Cancelled" };
            var appointments = new Faker<Appointment>("vi")
                .RuleFor(a => a.Id, f => Guid.NewGuid().ToString())
                .RuleFor(a => a.PropertyId, f => f.PickRandom(properties).Id)
                .RuleFor(a => a.UserId, f => f.PickRandom(tenants).Id)
                .RuleFor(a => a.AppointmentDateTime, f => f.Date.Future(1))
                .RuleFor(a => a.Status, f => f.PickRandom(appStatuses))
                .RuleFor(a => a.Note, f => f.Lorem.Sentence())
                .Generate(25);
            context.Appointments.AddRange(appointments);
        }

        private static void SeedRatings(AppDbContext context, List<Property> properties, List<User> tenants)
        {
            var attitudes = new[] { "Positive", "Negative", "Neutral" };
            var ratings = new Faker<Rating>("vi")
                .RuleFor(r => r.Id, f => Guid.NewGuid().ToString())
                .RuleFor(r => r.TenantId, f => f.PickRandom(tenants).Id)
                .RuleFor(r => r.PropertyId, f => f.PickRandom(properties).Id)
                .RuleFor(r => r.Stars, f => f.Random.Int(1, 5))
                .RuleFor(r => r.Content, f => f.Lorem.Sentence())
                .RuleFor(r => r.AIAttitude, f => f.PickRandom(attitudes))
                .Generate(20);
            context.Ratings.AddRange(ratings);
        }

        private static void SeedComplaints(AppDbContext context, List<User> tenants)
        {
            var cStatuses = new[] { "Pending", "Processing", "Resolved" };
            var cRelatedTypes = new[] { "Invoice", "Contract", "Property" };
            var complaints = new Faker<Complaint>("vi")
                .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
                .RuleFor(c => c.CreatorId, f => f.PickRandom(tenants).Id)
                .RuleFor(c => c.RelatedType, f => f.PickRandom(cRelatedTypes))
                .RuleFor(c => c.RelatedId, f => Guid.NewGuid().ToString())
                .RuleFor(c => c.Title, f => f.Lorem.Sentence(3))
                .RuleFor(c => c.Content, f => f.Lorem.Paragraph())
                .RuleFor(c => c.Status, f => f.PickRandom(cStatuses))
                .Generate(15);
            context.Complaints.AddRange(complaints);
        }

        private static void SeedMessages(AppDbContext context, List<User> users)
        {
            var messages = new Faker<Message>("vi")
                .RuleFor(m => m.Id, f => Guid.NewGuid().ToString())
                .RuleFor(m => m.SenderId, f => f.PickRandom(users).Id)
                .RuleFor(m => m.ReceiverId, f => f.PickRandom(users).Id)
                .RuleFor(m => m.Content, f => f.Lorem.Sentence())
                .RuleFor(m => m.IsRead, f => f.Random.Bool())
                .Generate(50);
            foreach(var m in messages)
            {
                if(m.SenderId == m.ReceiverId) m.ReceiverId = users.First(u => u.Id != m.SenderId).Id;
            }
            context.Messages.AddRange(messages);
        }

        private static void SeedNotifications(AppDbContext context, List<User> users)
        {
            var notificationTypes = new[] { "Invoice", "Appointment", "Contract", "System" };
            var notifications = new Faker<Notification>("vi")
                .RuleFor(n => n.Id, f => Guid.NewGuid().ToString())
                .RuleFor(n => n.UserId, f => f.PickRandom(users).Id)
                .RuleFor(n => n.Type, f => f.PickRandom(notificationTypes))
                .RuleFor(n => n.Content, f => f.Lorem.Sentence(5))
                .RuleFor(n => n.IsRead, f => f.Random.Bool())
                .Generate(30);
            context.Notifications.AddRange(notifications);
        }
    }
}
