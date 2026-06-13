using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    /// <summary>
    /// Dịch vụ nền tự động gửi thông báo nhắc nhở khi hóa đơn hoặc hợp đồng sắp đến hạn.
    /// Chạy mỗi 12 giờ một lần.
    /// </summary>
    public class NotificationReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationReminderBackgroundService> _logger;

        // Thời gian chạy định kỳ: mỗi 12 giờ
        private static readonly TimeSpan RunInterval = TimeSpan.FromHours(12);

        // Ngưỡng nhắc hóa đơn: 2 ngày trước hạn
        private const int InvoiceWarningDays = 2;

        // Ngưỡng nhắc hợp đồng: 30 ngày trước khi hết hạn
        private const int ContractWarningDays = 30;

        public NotificationReminderBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationReminderBackgroundService đã khởi động.");

            // Chạy lần đầu ngay khi khởi động (sau 10 giây chờ app sẵn sàng)
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunReminderJobAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Lỗi trong NotificationReminderBackgroundService.");
                }

                // Chờ 12 giờ trước lần chạy tiếp theo
                await Task.Delay(RunInterval, stoppingToken);
            }

            _logger.LogInformation("NotificationReminderBackgroundService đã dừng.");
        }

        private async Task RunReminderJobAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.UtcNow;
            _logger.LogInformation("Đang chạy job nhắc nhở thông báo lúc {Time}.", now);

            await SendInvoiceRemindersAsync(db, now, cancellationToken);
            await SendContractExpiryRemindersAsync(db, now, cancellationToken);

            _logger.LogInformation("Hoàn thành job nhắc nhở thông báo.");
        }

        /// <summary>
        /// Gửi thông báo nhắc nhở cho các hóa đơn Pending sắp đến hạn thanh toán.
        /// Tránh gửi trùng bằng cách kiểm tra thông báo đã tồn tại cho RelatedId đó.
        /// </summary>
        private async Task SendInvoiceRemindersAsync(
            AppDbContext db, DateTime now, CancellationToken cancellationToken)
        {
            var warningDeadline = now.AddDays(InvoiceWarningDays);

            // Lấy các hóa đơn Pending có DueDate trong [now, now + 2 ngày]
            var pendingInvoices = await db.Invoices
                .AsNoTracking()
                .Include(inv => inv.Contract)
                    .ThenInclude(c => c.Property)
                .Where(inv =>
                    inv.Status == InvoiceStatus.Pending &&
                    inv.DueDate >= now &&
                    inv.DueDate <= warningDeadline)
                .ToListAsync(cancellationToken);

            if (!pendingInvoices.Any())
            {
                _logger.LogInformation("Không có hóa đơn nào cần nhắc nhở.");
                return;
            }

            // Lấy tất cả RelatedId của thông báo Invoice đã tồn tại để tránh gửi trùng
            var existingReminderIds = await db.Notifications
                .AsNoTracking()
                .Where(n => n.Type == NotificationType.Invoice &&
                            n.Content.Contains("sắp đến hạn"))
                .Select(n => n.RelatedId)
                .ToHashSetAsync(cancellationToken);

            var newNotifications = new List<Notification>();

            foreach (var invoice in pendingInvoices)
            {
                // Bỏ qua nếu đã có thông báo nhắc nhở cho hóa đơn này
                if (existingReminderIds.Contains(invoice.Id))
                    continue;

                var tenantId = invoice.Contract?.TenantId;
                if (string.IsNullOrWhiteSpace(tenantId))
                    continue;

                var propertyName = invoice.Contract?.Property?.PropertyName ?? "phòng trọ";
                var daysLeft = (invoice.DueDate - now).Days;
                var dayText = daysLeft == 0 ? "hôm nay" : $"trong {daysLeft} ngày nữa";

                newNotifications.Add(new Notification
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = tenantId,
                    Type = NotificationType.Invoice,
                    Content = $"⚠️ Hóa đơn {invoice.Period:MM/yyyy} cho {propertyName} sắp đến hạn thanh toán ({dayText}). Tổng cộng {invoice.Total:N0}đ, hạn {invoice.DueDate:dd/MM/yyyy}.",
                    IsRead = false,
                    Timestamp = DateTime.UtcNow,
                    RelatedId = invoice.Id
                });
            }

            if (newNotifications.Any())
            {
                db.Notifications.AddRange(newNotifications);
                await db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Đã gửi {Count} thông báo nhắc hóa đơn.", newNotifications.Count);
            }
        }

        /// <summary>
        /// Gửi thông báo nhắc nhở cho các hợp đồng Active/NearExpiry sắp hết hạn.
        /// Tránh gửi trùng bằng cách kiểm tra thông báo đã tồn tại cho RelatedId đó.
        /// </summary>
        private async Task SendContractExpiryRemindersAsync(
            AppDbContext db, DateTime now, CancellationToken cancellationToken)
        {
            var warningDeadline = now.AddDays(ContractWarningDays);

            // Lấy các hợp đồng Active/NearExpiry có EndDate trong [now, now + 30 ngày]
            var nearExpiryContracts = await db.Contracts
                .AsNoTracking()
                .Include(c => c.Property)
                .Where(c =>
                    (c.Status == ContractStatus.Active || c.Status == ContractStatus.NearExpiry) &&
                    c.EndDate >= now &&
                    c.EndDate <= warningDeadline)
                .ToListAsync(cancellationToken);

            if (!nearExpiryContracts.Any())
            {
                _logger.LogInformation("Không có hợp đồng nào cần nhắc nhở.");
                return;
            }

            // Lấy các RelatedId đã được nhắc nhở để tránh gửi trùng
            var existingReminderIds = await db.Notifications
                .AsNoTracking()
                .Where(n => n.Type == NotificationType.Contract &&
                            n.Content.Contains("sắp hết hạn"))
                .Select(n => n.RelatedId)
                .ToHashSetAsync(cancellationToken);

            var newNotifications = new List<Notification>();

            foreach (var contract in nearExpiryContracts)
            {
                if (existingReminderIds.Contains(contract.Id))
                    continue;

                if (string.IsNullOrWhiteSpace(contract.TenantId))
                    continue;

                var propertyName = contract.Property?.PropertyName ?? "phòng trọ";
                var daysLeft = (contract.EndDate - now).Days;

                // Cập nhật trạng thái hợp đồng thành NearExpiry nếu chưa cập nhật
                if (contract.Status == ContractStatus.Active)
                {
                    var contractToUpdate = await db.Contracts.FindAsync(
                        new object[] { contract.Id }, cancellationToken);
                    if (contractToUpdate != null && contractToUpdate.Status == ContractStatus.Active)
                    {
                        contractToUpdate.Status = ContractStatus.NearExpiry;
                        contractToUpdate.UpdatedAt = DateTime.UtcNow;
                    }
                }

                newNotifications.Add(new Notification
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = contract.TenantId,
                    Type = NotificationType.Contract,
                    Content = $"⚠️ Hợp đồng thuê {propertyName} sắp hết hạn vào ngày {contract.EndDate:dd/MM/yyyy} (còn {daysLeft} ngày). Vui lòng liên hệ chủ trọ để gia hạn.",
                    IsRead = false,
                    Timestamp = DateTime.UtcNow,
                    RelatedId = contract.Id
                });
            }

            if (newNotifications.Any())
            {
                db.Notifications.AddRange(newNotifications);
                await db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Đã gửi {Count} thông báo nhắc hợp đồng.", newNotifications.Count);
            }
        }
    }
}
