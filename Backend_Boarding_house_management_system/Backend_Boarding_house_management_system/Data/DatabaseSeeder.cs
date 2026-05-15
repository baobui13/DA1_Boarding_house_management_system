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
        private sealed record SeedAreaDefinition(
            string Name,
            string Address,
            decimal Latitude,
            decimal Longitude,
            int RoomCount,
            string Description
        );

        private sealed record SeedPropertyDefinition(
            string PropertyName,
            int AreaIndex,
            string Address,
            decimal Latitude,
            decimal Longitude,
            decimal Size,
            decimal Price,
            decimal ElectricPrice,
            decimal WaterPrice,
            AvailabilityStatusEnum AvailabilityStatus,
            ModerationStatusEnum ModerationStatus,
            string Description,
            string? RejectionReason,
            string[] Amenities,
            string[] Images
        );

        private sealed record DemoUsers(
            User Admin,
            User Landlord,
            User Tenant
        );

        private static DateTime EnsureUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        public static async Task SeedDataAsync(AppDbContext context)
        {
            var hadExistingUsers = await context.Users.AnyAsync();
            var hasher = new PasswordHasher<User>();

            await EnsureDemoAccountsAsync(context, hasher);

            // Kiểm tra nếu hệ thống đã có dữ liệu thì không seed lại
            if (hadExistingUsers) return;
            var demoUsers = await GetDemoUsersAsync(context);

            var areas = SeedAreas(context, demoUsers.Landlord);
            var properties = SeedProperties(context, demoUsers.Landlord, areas);
            var amenities = SeedAmenities(context);
            SeedPropertyImages(context, properties);

            SeedRoomAmenities(context, properties, amenities);

            var contracts = SeedContracts(context, properties, demoUsers.Tenant);
            var invoices = SeedInvoices(context, contracts, properties);
            SeedPayments(context, invoices);

            SeedAppointments(context, properties, demoUsers.Tenant);
            SeedRatings(context, properties, demoUsers.Tenant);
            SeedComplaints(context, demoUsers.Tenant, contracts, invoices, properties);
            SeedMessages(context, demoUsers, properties, contracts);
            SeedNotifications(context, new List<User> { demoUsers.Admin, demoUsers.Landlord, demoUsers.Tenant }, properties, contracts, invoices, context.Appointments.Local.ToList());

            // Lưu tất cả thay đổi vào database
            await context.SaveChangesAsync();
        }

        private static async Task EnsureDemoAccountsAsync(AppDbContext context, PasswordHasher<User> hasher)
        {
            var demoUsers = new[]
            {
                new User
                {
                    UserName = "admin@test.com",
                    Email = "admin@test.com",
                    NormalizedEmail = "ADMIN@TEST.COM",
                    NormalizedUserName = "ADMIN@TEST.COM",
                    FullName = "System Admin",
                    CCCD = "000000000000",
                    Address = "Văn phòng quản trị hệ thống, Quận 1, TP. Hồ Chí Minh",
                    PhoneNumber = "0901000001",
                    Role = "Admin",
                    ReputationScore = 100,
                    EmailConfirmed = true
                },
                new User
                {
                    UserName = "landlord@test.com",
                    Email = "landlord@test.com",
                    NormalizedEmail = "LANDLORD@TEST.COM",
                    NormalizedUserName = "LANDLORD@TEST.COM",
                    FullName = "Demo Landlord",
                    CCCD = "111111111111",
                    Address = "123 Đinh Tiên Hoàng, Phường 1, Quận Bình Thạnh, TP. Hồ Chí Minh",
                    PhoneNumber = "0901000002",
                    Role = "Landlord",
                    ReputationScore = 86,
                    EmailConfirmed = true
                },
                new User
                {
                    UserName = "tenant@test.com",
                    Email = "tenant@test.com",
                    NormalizedEmail = "TENANT@TEST.COM",
                    NormalizedUserName = "TENANT@TEST.COM",
                    FullName = "Demo Tenant",
                    CCCD = "222222222222",
                    Address = "Phòng đang thuê tại Bình Thạnh, TP. Hồ Chí Minh",
                    PhoneNumber = "0901000003",
                    Role = "Tenant",
                    ReputationScore = 72,
                    EmailConfirmed = true
                }
            };

            foreach (var demoUser in demoUsers)
            {
                var existingUser = await context.Users.FirstOrDefaultAsync(user => user.Email == demoUser.Email);

                if (existingUser == null)
                {
                    demoUser.Id = Guid.NewGuid().ToString();
                    demoUser.CreatedAt = DateTime.UtcNow;
                    demoUser.UpdatedAt = DateTime.UtcNow;
                    demoUser.PasswordHash = hasher.HashPassword(demoUser, "Password123!");
                    context.Users.Add(demoUser);
                    continue;
                }

                existingUser.UserName = demoUser.UserName;
                existingUser.NormalizedEmail = demoUser.NormalizedEmail;
                existingUser.NormalizedUserName = demoUser.NormalizedUserName;
                existingUser.FullName = demoUser.FullName;
                existingUser.CCCD = string.IsNullOrWhiteSpace(existingUser.CCCD) ? demoUser.CCCD : existingUser.CCCD;
                existingUser.Address = demoUser.Address;
                existingUser.PhoneNumber = demoUser.PhoneNumber;
                existingUser.Role = demoUser.Role;
                existingUser.ReputationScore = demoUser.ReputationScore;
                existingUser.EmailConfirmed = true;
                existingUser.PasswordHash = hasher.HashPassword(existingUser, "Password123!");
                existingUser.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
        }

        private static async Task<DemoUsers> GetDemoUsersAsync(AppDbContext context)
        {
            var admin = await context.Users.FirstAsync(user => user.Email == "admin@test.com");
            var landlord = await context.Users.FirstAsync(user => user.Email == "landlord@test.com");
            var tenant = await context.Users.FirstAsync(user => user.Email == "tenant@test.com");

            return new DemoUsers(admin, landlord, tenant);
        }

        public static async Task ReseedPropertyCatalogAsync(AppDbContext context)
        {
            var hasher = new PasswordHasher<User>();
            await EnsureDemoAccountsAsync(context, hasher);
            var demoUsers = await GetDemoUsersAsync(context);
            var demoEmails = new[] { "admin@test.com", "landlord@test.com", "tenant@test.com" };

            await context.Payments.ExecuteDeleteAsync();
            await context.Messages.ExecuteDeleteAsync();
            await context.ViewHistories.ExecuteDeleteAsync();
            await context.SearchHistories.ExecuteDeleteAsync();
            await context.Ratings.ExecuteDeleteAsync();
            await context.Appointments.ExecuteDeleteAsync();
            await context.Complaints.ExecuteDeleteAsync();
            await context.TenantDocuments.ExecuteDeleteAsync();
            await context.RefreshTokens.ExecuteDeleteAsync();
            await context.RoomAmenities.ExecuteDeleteAsync();
            await context.PropertyImages.ExecuteDeleteAsync();
            await context.Invoices.ExecuteDeleteAsync();
            await context.Contracts.ExecuteDeleteAsync();
            await context.Properties.ExecuteDeleteAsync();
            await context.Areas.ExecuteDeleteAsync();
            await context.Amenities.ExecuteDeleteAsync();
            await context.Notifications.ExecuteDeleteAsync();
            await context.Users.Where(user => !demoEmails.Contains(user.Email!)).ExecuteDeleteAsync();

            var areas = SeedAreas(context, demoUsers.Landlord);
            var properties = SeedProperties(context, demoUsers.Landlord, areas);
            var amenities = SeedAmenities(context);
            SeedPropertyImages(context, properties);
            SeedRoomAmenities(context, properties, amenities);

            var contracts = SeedContracts(context, properties, demoUsers.Tenant);
            var invoices = SeedInvoices(context, contracts, properties);
            SeedPayments(context, invoices);
            SeedAppointments(context, properties, demoUsers.Tenant);
            SeedRatings(context, properties, demoUsers.Tenant);
            SeedComplaints(context, demoUsers.Tenant, contracts, invoices, properties);
            SeedMessages(context, demoUsers, properties, contracts);
            SeedNotifications(context, new List<User> { demoUsers.Admin, demoUsers.Landlord, demoUsers.Tenant }, properties, contracts, invoices, context.Appointments.Local.ToList());

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

        private static List<Area> SeedAreas(AppDbContext context, User landlord)
        {
            var areaDefinitions = GetSeedAreaDefinitions();
            var areas = areaDefinitions
                .Select(definition => new Area
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = definition.Name,
                    Address = definition.Address,
                    Latitude = definition.Latitude,
                    Longitude = definition.Longitude,
                    LandlordId = landlord.Id,
                    RoomCount = definition.RoomCount,
                    Description = definition.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                })
                .ToList();
                
            context.Areas.AddRange(areas);
            return areas;
        }

        private static List<Property> SeedProperties(AppDbContext context, User landlord, List<Area> areas)
        {
            var definitions = GetSeedPropertyDefinitions();
            var properties = definitions
                .Select(definition =>
                {
                    var area = areas[definition.AreaIndex];
                    return new Property
                    {
                        Id = Guid.NewGuid().ToString(),
                        LandlordId = landlord.Id,
                        AreaId = area.Id,
                        PropertyName = definition.PropertyName,
                        Address = definition.Address,
                        Latitude = definition.Latitude,
                        Longitude = definition.Longitude,
                        Size = definition.Size,
                        Description = definition.Description,
                        Price = definition.Price,
                        ElectricPrice = definition.ElectricPrice,
                        WaterPrice = definition.WaterPrice,
                        AvailabilityStatus = definition.AvailabilityStatus,
                        ModerationStatus = definition.ModerationStatus,
                        ApprovedAt = definition.ModerationStatus == ModerationStatusEnum.Approved ? DateTime.UtcNow : null,
                        RejectedAt = definition.ModerationStatus == ModerationStatusEnum.Rejected ? DateTime.UtcNow : null,
                        RejectionReason = definition.RejectionReason,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    };
                })
                .ToList();

            context.Properties.AddRange(properties);
            return properties;
        }

        private static List<Amenity> SeedAmenities(AppDbContext context)
        {
            var amenityNames = new[]
            {
                "Wifi",
                "Điều hòa",
                "Tủ lạnh",
                "Máy giặt",
                "Nóng lạnh",
                "Giường",
                "Tủ quần áo",
                "Camera an ninh",
                "Thang máy",
                "Ban công",
                "WC riêng",
                "Gác lửng",
                "Giữ xe",
                "Bếp riêng",
                "Cho nuôi thú cưng"
            };
            var amenities = amenityNames.Select(n => new Amenity
            {
                Id = Guid.NewGuid().ToString(),
                Name = n,
                Description = "Tiện ích " + n
            }).ToList();
            context.Amenities.AddRange(amenities);
            return amenities;
        }

        private static void SeedPropertyImages(AppDbContext context, List<Property> properties)
        {
            var imageDefinitions = GetSeedPropertyDefinitions();
            var propertyImages = new List<PropertyImage>();

            for (int i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                foreach (var item in imageDefinitions[i].Images.Select((imageUrl, imageIndex) => new { imageUrl, imageIndex }))
                {
                    propertyImages.Add(new PropertyImage
                    {
                        Id = Guid.NewGuid().ToString(),
                        PropertyId = property.Id,
                        ImageUrl = item.imageUrl,
                        PublicId = $"seed-property-{i + 1}-{item.imageIndex + 1}",
                        IsPrimary = item.imageIndex == 0,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            context.PropertyImages.AddRange(propertyImages);
        }

        private static void SeedRoomAmenities(AppDbContext context, List<Property> properties, List<Amenity> amenities)
        {
            var roomAmenities = new List<RoomAmenity>();
            var amenityMap = amenities.ToDictionary(item => item.Name, item => item);
            var definitions = GetSeedPropertyDefinitions();

            for (int i = 0; i < properties.Count; i++)
            {
                var p = properties[i];
                var pickedAmenities = definitions[i].Amenities
                    .Distinct()
                    .Where(amenityMap.ContainsKey)
                    .Select(name => amenityMap[name])
                    .ToList();

                if (pickedAmenities.Count == 0)
                {
                    pickedAmenities = amenities.Take(4).ToList();
                }

                foreach (var a in pickedAmenities)
                {
                    roomAmenities.Add(new RoomAmenity
                    {
                        Id = Guid.NewGuid().ToString(),
                        PropertyId = p.Id,
                        AmenityId = a.Id,
                        Status = "Working"
                    });
                }
            }
            context.RoomAmenities.AddRange(roomAmenities);
        }

        private static List<Contract> SeedContracts(AppDbContext context, List<Property> properties, User tenant)
        {
            var propertyByName = properties.ToDictionary(item => item.PropertyName, item => item);
            var now = DateTime.UtcNow;

            var contractBlueprints = new[]
            {
                new
                {
                    PropertyName = "Studio ban công Đinh Tiên Hoàng",
                    StartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2027, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                    Deposit = 9600000m,
                    Status = "Active",
                    Terms = "Thanh toán vào ngày 5 hàng tháng. Không hút thuốc trong phòng. Giữ yên tĩnh sau 22h."
                },
                new
                {
                    PropertyName = "Phòng gác lửng gần HUTECH",
                    StartDate = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                    Deposit = 8600000m,
                    Status = "Expired",
                    Terms = "Hợp đồng trước đây của khách thuê trong thời gian còn là sinh viên, đã bàn giao phòng đầy đủ."
                },
                new
                {
                    PropertyName = "Studio full nội thất Nguyễn Thị Thập",
                    StartDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    Deposit = 13000000m,
                    Status = "Terminated",
                    Terms = "Khách thuê chuyển nơi làm việc nên chấm dứt sớm, đã đối soát điện nước và bàn giao nội thất."
                }
            };

            var contracts = contractBlueprints
                .Where(item => propertyByName.ContainsKey(item.PropertyName))
                .Select(item =>
                {
                    var property = propertyByName[item.PropertyName];
                    return new Contract
                    {
                        Id = Guid.NewGuid().ToString(),
                        PropertyId = property.Id,
                        TenantId = tenant.Id,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        Deposit = item.Deposit,
                        Terms = item.Terms,
                        Status = item.Status,
                        CreatedAt = now,
                        UpdatedAt = now,
                        ActualEndDate = item.Status == "Expired" ? DateOnly.FromDateTime(item.EndDate) : item.Status == "Terminated" ? new DateOnly(2025, 12, 20) : null,
                        HandoverNote = item.Status == "Terminated" ? "Khách thuê chuyển việc sang quận khác, bàn giao sớm và đã chốt công nợ." : item.Status == "Expired" ? "Đã bàn giao đúng hạn, phòng sạch và hoàn trả đủ trang thiết bị." : null,
                        DeductionAmount = item.Status == "Terminated" ? 500000m : 0m,
                        DeductionReason = item.Status == "Terminated" ? "Khấu trừ vệ sinh tổng quát và thay khóa cửa." : null,
                        RefundAmount = item.Status == "Expired" ? item.Deposit : item.Status == "Terminated" ? item.Deposit - 500000m : 0m,
                        HandoverConfirmedBy = item.Status == "Active" ? null : "System Admin",
                        HandoverConfirmedAt = item.Status == "Active" ? null : now,
                    };
                })
                .ToList();

            foreach (var contract in contracts.Where(item => item.Status == "Active"))
            {
                propertyByName.First(pair => pair.Value.Id == contract.PropertyId).Value.AvailabilityStatus = AvailabilityStatusEnum.Rented;
            }

            context.Contracts.AddRange(contracts);
            return contracts;
        }

        private static List<Invoice> SeedInvoices(AppDbContext context, List<Contract> contracts, List<Property> properties)
        {
            var propertyById = properties.ToDictionary(item => item.Id, item => item);
            var contractByPropertyName = contracts.ToDictionary(
                contract => propertyById[contract.PropertyId].PropertyName,
                contract => contract
            );

            static Invoice CreateInvoice(
                Contract contract,
                Property property,
                int year,
                int month,
                decimal oldElectric,
                decimal newElectric,
                decimal oldWater,
                decimal newWater,
                decimal electricUnitPrice,
                decimal waterUnitPrice,
                decimal otherFees,
                decimal penalty,
                string status,
                string? note = null)
            {
                var electricityCost = (newElectric - oldElectric) * electricUnitPrice;
                var waterCost = (newWater - oldWater) * waterUnitPrice;
                var total = property.Price + electricityCost + waterCost + otherFees + penalty;
                var period = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);

                return new Invoice
                {
                    Id = Guid.NewGuid().ToString(),
                    ContractId = contract.Id,
                    Period = period,
                    RentAmount = property.Price,
                    OldElectricityReading = oldElectric,
                    NewElectricityReading = newElectric,
                    ElectricityCost = electricityCost,
                    OldWaterReading = oldWater,
                    NewWaterReading = newWater,
                    WaterCost = waterCost,
                    OtherFees = otherFees,
                    Penalty = penalty,
                    Total = total,
                    Note = note,
                    Status = status,
                    DueDate = period.AddMonths(1).AddDays(4),
                    CreatedAt = period.AddDays(1),
                    UpdatedAt = status == "Paid" || status == "Partial" ? period.AddMonths(1).AddDays(2) : null,
                    ReceiptUrl = status == "Paid" ? "https://example.com/receipts/demo-paid.pdf" : null,
                    InvoiceUrl = null
                };
            }

            var invoices = new List<Invoice>();

            if (contractByPropertyName.TryGetValue("Studio ban công Đinh Tiên Hoàng", out var dinhTienHoangContract))
            {
                var property = propertyById[dinhTienHoangContract.PropertyId];
                invoices.Add(CreateInvoice(dinhTienHoangContract, property, 2026, 3, 1170, 1240, 21, 24, 3500m, 15000m, 200000m, 0m, "Paid", "Tháng đầu khách vào ở ổn định."));
                invoices.Add(CreateInvoice(dinhTienHoangContract, property, 2026, 4, 1240, 1315, 24, 27, 3500m, 15000m, 200000m, 0m, "Paid"));
                invoices.Add(CreateInvoice(dinhTienHoangContract, property, 2026, 5, 1315, 1392, 27, 30, 3500m, 15000m, 200000m, 0m, "Pending", "Khách thuê hẹn thanh toán vào cuối tuần này."));
            }

            if (contractByPropertyName.TryGetValue("Phòng gác lửng gần HUTECH", out var hutechContract))
            {
                var property = propertyById[hutechContract.PropertyId];
                invoices.Add(CreateInvoice(hutechContract, property, 2025, 12, 790, 860, 15, 18, 3200m, 14000m, 150000m, 0m, "Paid", "Hóa đơn tháng cuối trước khi hết hạn hợp đồng."));
            }

            if (contractByPropertyName.TryGetValue("Studio full nội thất Nguyễn Thị Thập", out var quan7Contract))
            {
                var property = propertyById[quan7Contract.PropertyId];
                invoices.Add(CreateInvoice(quan7Contract, property, 2025, 11, 520, 610, 12, 14, 3800m, 18000m, 300000m, 0m, "Paid"));
                invoices.Add(CreateInvoice(quan7Contract, property, 2025, 12, 610, 650, 14, 15, 3800m, 18000m, 300000m, 150000m, "Partial", "Khách thuê trả trước phần lớn, phần còn lại được trừ vào cọc khi chấm dứt hợp đồng."));
            }

            context.Invoices.AddRange(invoices);
            return invoices;
        }

        private static void SeedPayments(AppDbContext context, List<Invoice> invoices)
        {
            var payments = new List<Payment>();
            foreach (var inv in invoices.Where(x => x.Status == "Paid"))
            {
                payments.Add(new Payment
                {
                    Id = Guid.NewGuid().ToString(),
                    InvoiceId = inv.Id,
                    Amount = inv.Total,
                    PaymentDate = EnsureUtc(inv.DueDate.AddDays(-2)),
                    Method = "BankTransfer",
                    CreatedAt = EnsureUtc(inv.DueDate.AddDays(-2)),
                    Note = "Thanh toán đủ hóa đơn"
                });
            }
            foreach (var inv in invoices.Where(x => x.Status == "Partial"))
            {
                payments.Add(new Payment
                {
                    Id = Guid.NewGuid().ToString(),
                    InvoiceId = inv.Id,
                    Amount = inv.Total / 2,
                    PaymentDate = EnsureUtc(inv.DueDate.AddDays(-1)),
                    Method = "Online",
                    CreatedAt = EnsureUtc(inv.DueDate.AddDays(-1)),
                    Note = "Thanh toán một phần"
                });
            }
            context.Payments.AddRange(payments);
        }

        private static void SeedAppointments(AppDbContext context, List<Property> properties, User tenant)
        {
            var propertyByName = properties.ToDictionary(item => item.PropertyName, item => item);

            var appointmentBlueprints = new[]
            {
                new
                {
                    PropertyName = "Studio ban công gần sân bay",
                    AppointmentDateTime = new DateTime(2026, 5, 14, 9, 30, 0, DateTimeKind.Utc),
                    Status = "Confirmed",
                    Note = "Khách thuê từng cân nhắc chuyển công tác gần sân bay nên đã hẹn xem phòng này."
                },
                new
                {
                    PropertyName = "Phòng máy lạnh Kha Vạn Cân",
                    AppointmentDateTime = new DateTime(2026, 5, 18, 18, 30, 0, DateTimeKind.Utc),
                    Status = "Pending",
                    Note = "Khách thuê hỏi giúp cho em họ đang tìm phòng gần khu Thủ Đức."
                },
                new
                {
                    PropertyName = "Phòng giá tốt Phan Văn Trị",
                    AppointmentDateTime = new DateTime(2026, 5, 9, 19, 0, 0, DateTimeKind.Utc),
                    Status = "Cancelled",
                    Note = "Khách thuê bận họp đột xuất nên đã chủ động hủy lịch."
                }
            };

            var appointments = appointmentBlueprints
                .Where(item => propertyByName.ContainsKey(item.PropertyName))
                .Select(item => new Appointment
                {
                    Id = Guid.NewGuid().ToString(),
                    PropertyId = propertyByName[item.PropertyName].Id,
                    UserId = tenant.Id,
                    AppointmentDateTime = item.AppointmentDateTime,
                    Status = item.Status,
                    Note = item.Note,
                    CreatedAt = item.AppointmentDateTime.AddDays(-2),
                    UpdatedAt = item.Status == "Pending" ? null : item.AppointmentDateTime.AddDays(-1)
                })
                .ToList();
            context.Appointments.AddRange(appointments);
        }

        private static void SeedRatings(AppDbContext context, List<Property> properties, User tenant)
        {
            var propertyByName = properties.ToDictionary(item => item.PropertyName, item => item);
            var ratings = new List<Rating>();

            if (propertyByName.TryGetValue("Phòng gác lửng gần HUTECH", out var hutechProperty))
            {
                ratings.Add(new Rating
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = tenant.Id,
                    PropertyId = hutechProperty.Id,
                    Stars = 5,
                    Content = "Phòng đúng mô tả, chủ hỗ trợ nhanh, vị trí rất tiện cho việc đi học và đi làm.",
                    AIAttitude = "Positive",
                    CreatedAt = new DateTime(2025, 12, 28, 8, 0, 0, DateTimeKind.Utc)
                });
            }

            if (propertyByName.TryGetValue("Studio ban công Đinh Tiên Hoàng", out var dinhTienHoangProperty))
            {
                ratings.Add(new Rating
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = tenant.Id,
                    PropertyId = dinhTienHoangProperty.Id,
                    Stars = 4,
                    Content = "Phòng sáng, sạch và yên tĩnh. Chỉ mong khu để xe rộng thêm một chút vào giờ cao điểm.",
                    AIAttitude = "Positive",
                    CreatedAt = new DateTime(2026, 4, 20, 10, 30, 0, DateTimeKind.Utc)
                });
            }

            context.Ratings.AddRange(ratings);
        }

        private static void SeedComplaints(AppDbContext context, User tenant, List<Contract> contracts, List<Invoice> invoices, List<Property> properties)
        {
            var propertyById = properties.ToDictionary(item => item.Id, item => item);
            var activeContract = contracts.FirstOrDefault(contract => contract.Status == "Active");
            var pendingInvoice = invoices.FirstOrDefault(invoice => invoice.Status == "Pending");
            var terminatedContract = contracts.FirstOrDefault(contract => contract.Status == "Terminated");
            var complaints = new List<Complaint>();

            if (pendingInvoice != null && activeContract != null)
            {
                complaints.Add(new Complaint
                {
                    Id = Guid.NewGuid().ToString(),
                    CreatorId = tenant.Id,
                    RelatedType = "Invoice",
                    RelatedId = pendingInvoice.Id,
                    Title = "Xin gia hạn ngày thanh toán hóa đơn tháng 05/2026",
                    Content = $"Khách thuê đã báo trước về việc lùi ngày nhận lương và mong được gia hạn thêm 3 ngày để thanh toán hóa đơn của {propertyById[activeContract.PropertyId].PropertyName}.",
                    Status = "Processing",
                    CreatedAt = new DateTime(2026, 5, 6, 9, 0, 0, DateTimeKind.Utc)
                });
            }

            if (terminatedContract != null)
            {
                complaints.Add(new Complaint
                {
                    Id = Guid.NewGuid().ToString(),
                    CreatorId = tenant.Id,
                    RelatedType = "Contract",
                    RelatedId = terminatedContract.Id,
                    Title = "Xác nhận khấu trừ tiền cọc sau khi chấm dứt hợp đồng",
                    Content = "Khách thuê đề nghị gửi biên bản ảnh trước và sau dọn phòng để đối chiếu khoản khấu trừ vệ sinh và thay khóa.",
                    Status = "Resolved",
                    AdminResponse = "Đã đối chiếu với chủ trọ và bổ sung biên bản bàn giao trong mục chi tiết hợp đồng.",
                    ResolvedAt = new DateTime(2026, 1, 5, 15, 0, 0, DateTimeKind.Utc),
                    CreatedAt = new DateTime(2025, 12, 22, 15, 0, 0, DateTimeKind.Utc)
                });
            }

            context.Complaints.AddRange(complaints);
        }

        private static void SeedMessages(AppDbContext context, DemoUsers demoUsers, List<Property> properties, List<Contract> contracts)
        {
            var currentProperty = properties.First(property => property.PropertyName == "Studio ban công Đinh Tiên Hoàng");
            var currentContract = contracts.First(contract => contract.PropertyId == currentProperty.Id);
            var messages = new List<Message>
            {
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    SenderId = demoUsers.Tenant.Id,
                    ReceiverId = demoUsers.Landlord.Id,
                    Content = "Anh/chị cho em xin xác nhận lịch kiểm tra đồng hồ điện nước vào cuối tháng này nhé.",
                    PropertyId = currentProperty.Id,
                    ContractId = currentContract.Id,
                    Timestamp = new DateTime(2026, 4, 28, 12, 30, 0, DateTimeKind.Utc),
                    IsRead = true
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    SenderId = demoUsers.Landlord.Id,
                    ReceiverId = demoUsers.Tenant.Id,
                    Content = "Tối 30/04 anh ghé chụp chỉ số, sau đó sẽ tạo hóa đơn đầu tháng cho em như thường lệ.",
                    PropertyId = currentProperty.Id,
                    ContractId = currentContract.Id,
                    Timestamp = new DateTime(2026, 4, 28, 12, 45, 0, DateTimeKind.Utc),
                    IsRead = true
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    SenderId = demoUsers.Tenant.Id,
                    ReceiverId = demoUsers.Landlord.Id,
                    Content = "Tháng này em nhận lương chậm 2 ngày, mong anh/chị cho em dời lịch thanh toán hóa đơn đến cuối tuần.",
                    PropertyId = currentProperty.Id,
                    ContractId = currentContract.Id,
                    Timestamp = new DateTime(2026, 5, 5, 8, 15, 0, DateTimeKind.Utc),
                    IsRead = false
                }
            };
            context.Messages.AddRange(messages);
        }

        private static void SeedNotifications(
            AppDbContext context,
            List<User> users,
            List<Property> properties,
            List<Contract> contracts,
            List<Invoice> invoices,
            List<Appointment> appointments)
        {
            var notifications = new List<Notification>();
            var propertyById = properties.ToDictionary(item => item.Id, item => item);
            var userById = users.ToDictionary(item => item.Id, item => item);

            foreach (var invoice in invoices)
            {
                var contract = contracts.First(item => item.Id == invoice.ContractId);
                var property = propertyById[contract.PropertyId];

                notifications.Add(new Notification
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = contract.TenantId,
                    Type = "Invoice",
                    Content = $"Hóa đơn {invoice.Period:MM/yyyy} cho {property.PropertyName} đã được tạo. Tổng cộng {invoice.Total:N0}đ, hạn thanh toán {invoice.DueDate:dd/MM/yyyy}.",
                    IsRead = invoice.Status == "Paid",
                    Timestamp = invoice.CreatedAt,
                    RelatedId = invoice.Id
                });

                if (invoice.Status == "Pending")
                {
                    notifications.Add(new Notification
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = contract.TenantId,
                        Type = "Invoice",
                        Content = $"Nhắc thanh toán hóa đơn {invoice.Period:MM/yyyy} cho {property.PropertyName}. Vui lòng thanh toán trước {invoice.DueDate:dd/MM/yyyy}.",
                        IsRead = false,
                        Timestamp = invoice.DueDate.AddDays(-2),
                        RelatedId = invoice.Id
                    });
                }
                else if (invoice.Status == "Partial")
                {
                    notifications.Add(new Notification
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = contract.TenantId,
                        Type = "Invoice",
                        Content = $"Hóa đơn {invoice.Period:MM/yyyy} cho {property.PropertyName} đã thanh toán một phần. Vui lòng hoàn tất số còn lại trước {invoice.DueDate:dd/MM/yyyy}.",
                        IsRead = false,
                        Timestamp = invoice.UpdatedAt ?? invoice.CreatedAt,
                        RelatedId = invoice.Id
                    });
                }
            }

            foreach (var appointment in appointments)
            {
                var property = propertyById[appointment.PropertyId];
                var tenantName = userById.TryGetValue(appointment.UserId, out var tenant) ? tenant.FullName : "Khách thuê";

                notifications.Add(new Notification
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = appointment.UserId,
                    Type = "Appointment",
                    Content = appointment.Status switch
                    {
                        "Confirmed" => $"Lịch xem phòng {property.PropertyName} lúc {appointment.AppointmentDateTime:HH:mm dd/MM/yyyy} đã được xác nhận.",
                        "Rejected" => $"Lịch xem phòng {property.PropertyName} lúc {appointment.AppointmentDateTime:HH:mm dd/MM/yyyy} đã bị từ chối.",
                        "Cancelled" => $"Lịch xem phòng {property.PropertyName} lúc {appointment.AppointmentDateTime:HH:mm dd/MM/yyyy} đã bị hủy.",
                        _ => $"Yêu cầu xem phòng {property.PropertyName} lúc {appointment.AppointmentDateTime:HH:mm dd/MM/yyyy} đang chờ xác nhận."
                    },
                    IsRead = appointment.Status == "Cancelled",
                    Timestamp = appointment.UpdatedAt ?? appointment.CreatedAt,
                    RelatedId = appointment.Id
                });

                notifications.Add(new Notification
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = property.LandlordId,
                    Type = "Appointment",
                    Content = $"Có yêu cầu xem phòng mới từ {tenantName} cho {property.PropertyName} vào lúc {appointment.AppointmentDateTime:HH:mm dd/MM/yyyy}.",
                    IsRead = appointment.Status != "Pending",
                    Timestamp = appointment.CreatedAt,
                    RelatedId = appointment.Id
                });
            }

            foreach (var contract in contracts)
            {
                var property = propertyById[contract.PropertyId];
                notifications.Add(new Notification
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = contract.TenantId,
                    Type = "Contract",
                    Content = contract.Status switch
                    {
                        "Expired" => $"Hợp đồng thuê {property.PropertyName} đã hết hạn vào ngày {contract.EndDate:dd/MM/yyyy}.",
                        "Terminated" => $"Hợp đồng thuê {property.PropertyName} đã được chấm dứt. Số tiền hoàn cọc dự kiến là {contract.RefundAmount:N0}đ.",
                        _ => $"Hợp đồng thuê {property.PropertyName} đang có hiệu lực đến ngày {contract.EndDate:dd/MM/yyyy}."
                    },
                    IsRead = contract.Status == "Active",
                    Timestamp = contract.CreatedAt,
                    RelatedId = contract.Id
                });
            }

            var systemTargets = users.Where(user => user.Role == "Tenant" || user.Role == "Landlord").Take(6);
            foreach (var user in systemTargets)
            {
                notifications.Add(new Notification
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    Type = "System",
                    Content = "Hệ thống đã cập nhật dữ liệu mẫu khu trọ tại TP. Hồ Chí Minh để bạn tiện kiểm tra giao diện và luồng nghiệp vụ.",
                    IsRead = false,
                    Timestamp = DateTime.UtcNow,
                    RelatedId = null
                });
            }

            context.Notifications.AddRange(notifications);
        }

        private static List<SeedAreaDefinition> GetSeedAreaDefinitions()
        {
            return new()
            {
                new("Khu trọ Bình Thạnh A", "123 Đinh Tiên Hoàng, Phường 1, Quận Bình Thạnh, TP. Hồ Chí Minh", 10.807030m, 106.715950m, 4, "Khu trọ gần HUTECH và Landmark 81, phù hợp sinh viên và người đi làm trẻ."),
                new("Khu trọ Quận 7 Riverside", "88 Nguyễn Thị Thập, Phường Tân Phú, Quận 7, TP. Hồ Chí Minh", 10.737310m, 106.717180m, 3, "Khu phòng hiện đại gần Crescent Mall, dễ di chuyển về Phú Mỹ Hưng."),
                new("Nhà trọ Thủ Đức Campus", "66 Kha Vạn Cân, Phường Linh Đông, TP. Thủ Đức, TP. Hồ Chí Minh", 10.857920m, 106.756120m, 3, "Phù hợp sinh viên khu vực Sư phạm Kỹ thuật và Đại học Ngân Hàng."),
                new("Khu trọ Tân Bình Airport", "12 Hậu Giang, Phường 4, Quận Tân Bình, TP. Hồ Chí Minh", 10.801260m, 106.659110m, 2, "Khu trọ gần sân bay Tân Sơn Nhất, thích hợp tiếp viên, nhân viên văn phòng."),
                new("Khu trọ Gò Vấp Parkside", "45 Phan Văn Trị, Phường 10, Quận Gò Vấp, TP. Hồ Chí Minh", 10.831990m, 106.679730m, 3, "Khu trọ gần công viên Gia Định, nhiều tiện ích ăn uống và xe buýt.")
            };
        }

        private static List<SeedPropertyDefinition> GetSeedPropertyDefinitions()
        {
            return new()
            {
                new("Studio ban công Đinh Tiên Hoàng", 0, "123 Đinh Tiên Hoàng, Phường 1, Quận Bình Thạnh, TP. Hồ Chí Minh", 10.807030m, 106.715950m, 24m, 4800000m, 3500m, 15000m, AvailabilityStatusEnum.Available, ModerationStatusEnum.Approved, "Studio sáng, có ban công nhỏ nhìn ra tuyến Đinh Tiên Hoàng, full máy lạnh và bếp riêng.", null, ["Wifi", "Điều hòa", "Ban công", "WC riêng", "Giữ xe"], new[] { "https://images.unsplash.com/photo-1737737196308-e5b848160b78?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200", "https://images.unsplash.com/photo-1764836168197-3aa3a890a0f0?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200", "https://images.unsplash.com/photo-1661796428175-55423b19409f?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200" }),
                new("Phòng gác lửng gần HUTECH", 0, "127 Đinh Tiên Hoàng, Phường 1, Quận Bình Thạnh, TP. Hồ Chí Minh", 10.807450m, 106.716420m, 28m, 4300000m, 3200m, 14000m, AvailabilityStatusEnum.Rented, ModerationStatusEnum.Approved, "Phòng có gác lửng, nội thất cơ bản, đi bộ 5 phút đến HUTECH cơ sở Ung Văn Khiêm.", null, ["Wifi", "Điều hòa", "Gác lửng", "WC riêng"], new[] { "https://images.unsplash.com/photo-1764836168197-3aa3a890a0f0?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200", "https://images.unsplash.com/photo-1737737196308-e5b848160b78?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200" }),
                new("Căn hộ mini view Landmark", 0, "129 Đinh Tiên Hoàng, Phường 1, Quận Bình Thạnh, TP. Hồ Chí Minh", 10.808010m, 106.717050m, 32m, 6200000m, 3800m, 18000m, AvailabilityStatusEnum.Maintenance, ModerationStatusEnum.Pending, "Căn hộ mini đang hoàn thiện lại nội thất, có view thoáng về phía Landmark 81.", null, ["Wifi", "Điều hòa", "Ban công", "Tủ lạnh", "Bếp riêng"], new[] { "https://images.unsplash.com/photo-1661796428175-55423b19409f?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200", "https://images.unsplash.com/photo-1602646994030-464f98de5e5c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200" }),
                new("Studio full nội thất Nguyễn Thị Thập", 1, "88 Nguyễn Thị Thập, Phường Tân Phú, Quận 7, TP. Hồ Chí Minh", 10.737310m, 106.717180m, 34m, 6500000m, 3800m, 18000m, AvailabilityStatusEnum.Available, ModerationStatusEnum.Approved, "Studio full nội thất, nằm trong khu dân cư yên tĩnh gần Crescent Mall.", null, ["Wifi", "Điều hòa", "Tủ lạnh", "Máy giặt", "Giữ xe"], new[] { "https://images.unsplash.com/photo-1661796428175-55423b19409f?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200", "https://images.unsplash.com/photo-1737737196308-e5b848160b78?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200" }),
                new("Phòng cửa sổ lớn Sunrise City", 1, "90 Nguyễn Thị Thập, Phường Tân Hưng, Quận 7, TP. Hồ Chí Minh", 10.739020m, 106.708950m, 26m, 5200000m, 3600m, 16000m, AvailabilityStatusEnum.Rented, ModerationStatusEnum.Approved, "Phòng nhiều ánh sáng tự nhiên, gần Lotte Mart Quận 7 và trục Nguyễn Hữu Thọ.", null, ["Wifi", "Điều hòa", "WC riêng", "Giữ xe"], new[] { "https://images.unsplash.com/photo-1771337744364-e7dd00c2921c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200", "https://images.unsplash.com/photo-1764836168197-3aa3a890a0f0?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200" }),
                new("Căn hộ mini cho nuôi thú cưng", 1, "92 Nguyễn Thị Thập, Phường Bình Thuận, Quận 7, TP. Hồ Chí Minh", 10.741220m, 106.706410m, 30m, 5900000m, 3600m, 17000m, AvailabilityStatusEnum.Maintenance, ModerationStatusEnum.Rejected, "Căn hộ mini cho phép nuôi thú cưng, có khu vực bếp và máy giặt riêng.", "Tin đăng bị từ chối do bộ ảnh chưa phản ánh đúng hiện trạng phòng và thiếu giấy tờ xác thực quyền cho thuê.", ["Wifi", "Điều hòa", "Cho nuôi thú cưng", "Máy giặt", "Bếp riêng"], new[] { "https://images.unsplash.com/photo-1602646994030-464f98de5e5c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200", "https://images.unsplash.com/photo-1661796428175-55423b19409f?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200" }),
                new("Phòng máy lạnh Kha Vạn Cân", 2, "66 Kha Vạn Cân, Phường Linh Đông, TP. Thủ Đức, TP. Hồ Chí Minh", 10.857920m, 106.756120m, 22m, 3400000m, 3000m, 14000m, AvailabilityStatusEnum.Available, ModerationStatusEnum.Pending, "Phòng máy lạnh thoáng, phù hợp sinh viên Thủ Đức, gần tuyến xe buýt về trung tâm.", null, ["Wifi", "Điều hòa", "WC riêng", "Giữ xe"], new[] { "https://images.unsplash.com/photo-1602646994030-464f98de5e5c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200", "https://images.unsplash.com/photo-1737737196308-e5b848160b78?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200" }),
                new("Phòng yên tĩnh gần ĐH SPKT", 2, "70 Kha Vạn Cân, Phường Linh Chiểu, TP. Thủ Đức, TP. Hồ Chí Minh", 10.850950m, 106.771250m, 27m, 3900000m, 3000m, 14000m, AvailabilityStatusEnum.Rented, ModerationStatusEnum.Approved, "Phòng sạch, khu dân cư an ninh, tiện đi SPKT và Vincom Thủ Đức.", null, ["Wifi", "Điều hòa", "WC riêng", "Giữ xe", "Nóng lạnh"], new[] { "https://images.unsplash.com/photo-1737737196308-e5b848160b78?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200", "https://images.unsplash.com/photo-1771337744364-e7dd00c2921c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200" }),
                new("Studio ban công gần sân bay", 3, "12 Hậu Giang, Phường 4, Quận Tân Bình, TP. Hồ Chí Minh", 10.801260m, 106.659110m, 25m, 5600000m, 3700m, 16000m, AvailabilityStatusEnum.Available, ModerationStatusEnum.Approved, "Studio có ban công, gần sân bay Tân Sơn Nhất và công viên Hoàng Văn Thụ.", null, ["Wifi", "Điều hòa", "Ban công", "Thang máy", "Giữ xe"], new[] { "https://images.unsplash.com/photo-1661796428175-55423b19409f?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200", "https://images.unsplash.com/photo-1764836168197-3aa3a890a0f0?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200" }),
                new("Phòng mới sửa khu Bảy Hiền", 3, "26 Cộng Hòa, Phường 13, Quận Tân Bình, TP. Hồ Chí Minh", 10.801640m, 106.644650m, 21m, 3600000m, 3100m, 14500m, AvailabilityStatusEnum.Maintenance, ModerationStatusEnum.Pending, "Phòng đang hoàn thiện cải tạo, dự kiến mở lại trong tháng với nội thất mới.", null, ["Wifi", "Điều hòa", "WC riêng"], new[] { "https://images.unsplash.com/photo-1771337744364-e7dd00c2921c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200", "https://images.unsplash.com/photo-1602646994030-464f98de5e5c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200" }),
                new("Phòng giá tốt Phan Văn Trị", 4, "45 Phan Văn Trị, Phường 10, Quận Gò Vấp, TP. Hồ Chí Minh", 10.831990m, 106.679730m, 20m, 2950000m, 2800m, 12000m, AvailabilityStatusEnum.Available, ModerationStatusEnum.Approved, "Phòng giá tốt gần Emart và công viên Gia Định, phù hợp người đi làm độc thân.", null, ["Wifi", "WC riêng", "Giữ xe"], new[] { "https://images.unsplash.com/photo-1771337744364-e7dd00c2921c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200", "https://images.unsplash.com/photo-1737737196308-e5b848160b78?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200" }),
                new("Phòng có bếp riêng công viên Gia Định", 4, "51 Phan Văn Trị, Phường 10, Quận Gò Vấp, TP. Hồ Chí Minh", 10.833100m, 106.680920m, 29m, 4100000m, 3200m, 15000m, AvailabilityStatusEnum.Available, ModerationStatusEnum.Rejected, "Phòng có bếp riêng và cửa sổ lớn, khu vực đông dân cư và tiện sinh hoạt.", "Tin đăng bị từ chối do mô tả diện tích chưa khớp hồ sơ phòng và thiếu thông tin liên hệ xác minh.", ["Wifi", "Bếp riêng", "WC riêng", "Cho nuôi thú cưng"], new[] { "https://images.unsplash.com/photo-1602646994030-464f98de5e5c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200", "https://images.unsplash.com/photo-1661796428175-55423b19409f?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200" })
            };
        }
    }
}
