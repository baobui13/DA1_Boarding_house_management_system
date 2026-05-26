using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backend_Boarding_house_management_system.DTOs.Chatbot.Requests;
using Backend_Boarding_house_management_system.DTOs.Chatbot.Responses;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class ChatbotService : IChatbotService
    {
        private readonly HttpClient _httpClient;
        private readonly string _aiServiceBaseUrl;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<ChatbotService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
        };

        public ChatbotService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            AppDbContext dbContext,
            ILogger<ChatbotService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("AiService");
            _aiServiceBaseUrl = configuration["AiService:BaseUrl"]
                ?? "http://localhost:8000";
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ChatResponse> ChatAsync(ChatRequest request, string? userId = null)
        {
            string contextString = string.Empty;

            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
                    if (user != null)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"Tên người dùng: {user.FullName} ({user.UserName})");
                        var isLandlord = string.Equals(request.UserRole, "landlord", StringComparison.OrdinalIgnoreCase);
                        sb.AppendLine($"Vai trò: {(isLandlord ? "Chủ trọ (Landlord)" : "Người thuê trọ (Tenant)")}");
                        sb.AppendLine($"Thời gian hệ thống hiện tại: {DateTime.Now:dd/MM/yyyy HH:mm}");

                        if (!isLandlord)
                        {
                            // 1. Lấy thông tin hợp đồng đang kích hoạt (gồm cả đang hoạt động và sắp hết hạn)
                            var contracts = await _dbContext.Contracts
                                .Include(c => c.Property)
                                .Where(c => c.TenantId == userId && (c.Status == ContractStatus.Active || c.Status == ContractStatus.NearExpiry))
                                .ToListAsync();

                            if (contracts.Any())
                            {
                                sb.AppendLine("\nDanh sách Hợp đồng đang hiệu lực:");
                                foreach (var c in contracts)
                                {
                                    sb.AppendLine($"- Hợp đồng số: {c.Id}");
                                    sb.AppendLine($"  Phòng trọ: {c.Property?.PropertyName}");
                                    sb.AppendLine($"  Địa chỉ: {c.Property?.Address}");
                                    sb.AppendLine($"  Ngày bắt đầu: {c.StartDate:dd/MM/yyyy}");
                                    sb.AppendLine($"  Ngày kết thúc: {c.EndDate:dd/MM/yyyy}");
                                    sb.AppendLine($"  Tiền đặt cọc: {c.Deposit:N0} VND");
                                    sb.AppendLine($"  Giá điện: {c.Property?.ElectricPrice:N0} VND/kWh");
                                    sb.AppendLine($"  Giá nước: {c.Property?.WaterPrice:N0} VND/m3");

                                    // 1a. Lấy danh sách tiện ích của phòng trọ hiện tại
                                    var roomAmenities = await _dbContext.RoomAmenities
                                        .Include(ra => ra.Amenity)
                                        .Where(ra => ra.PropertyId == c.PropertyId)
                                        .ToListAsync();

                                    if (roomAmenities.Any())
                                    {
                                        sb.AppendLine("  Tiện ích trong phòng trọ:");
                                        foreach (var ra in roomAmenities)
                                        {
                                            string statusStr = ra.Status switch
                                            {
                                                AmenityStatus.Working => "Đang hoạt động tốt",
                                                AmenityStatus.Broken => "Bị hỏng",
                                                AmenityStatus.Repairing => "Đang sửa chữa",
                                                _ => "Bình thường"
                                            };
                                            sb.AppendLine($"    * Tiện ích: {ra.Amenity?.Name} | Trạng thái: {statusStr}{(string.IsNullOrEmpty(ra.Note) ? "" : $" (Ghi chú: {ra.Note})")}");
                                        }
                                    }

                                    // 1b. Lấy 3 hóa đơn gần nhất của hợp đồng này
                                    var invoices = await _dbContext.Invoices
                                        .Where(i => i.ContractId == c.Id)
                                        .OrderByDescending(i => i.Period)
                                        .Take(3)
                                        .ToListAsync();

                                    if (invoices.Any())
                                    {
                                        sb.AppendLine("  Hóa đơn gần đây:");
                                        foreach (var inv in invoices)
                                        {
                                            sb.AppendLine($"    * Hóa đơn kì tháng: {inv.Period:MM/yyyy}");
                                            sb.AppendLine($"      Mã hóa đơn: {inv.Id}");
                                            sb.AppendLine($"      Tiền phòng: {inv.RentAmount:N0} VND");
                                            sb.AppendLine($"      Tiền điện: {inv.ElectricityCost:N0} VND (Số cũ: {inv.OldElectricityReading}, Số mới: {inv.NewElectricityReading})");
                                            sb.AppendLine($"      Tiền nước: {inv.WaterCost:N0} VND (Số cũ: {inv.OldWaterReading}, Số mới: {inv.NewWaterReading})");
                                            if (inv.OtherFees > 0) sb.AppendLine($"      Chi phí khác: {inv.OtherFees:N0} VND");
                                            sb.AppendLine($"      Tổng tiền: {inv.Total:N0} VND");
                                            sb.AppendLine($"      Hạn thanh toán: {inv.DueDate:dd/MM/yyyy}");
                                            sb.AppendLine($"      Trạng thái thanh toán: {(inv.Status == InvoiceStatus.Paid ? "Đã thanh toán" : inv.Status == InvoiceStatus.Partial ? "Thanh toán một phần" : "Chưa thanh toán")}");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                sb.AppendLine("Không tìm thấy hợp đồng đang hiệu lực.");
                            }

                            // 2. Lấy danh sách lịch hẹn của Tenant
                            var appointments = await _dbContext.Appointments
                                .Include(a => a.Property)
                                .Where(a => a.UserId == userId)
                                .OrderByDescending(a => a.AppointmentDateTime)
                                .Take(5)
                                .ToListAsync();

                            if (appointments.Any())
                            {
                                sb.AppendLine("\nDanh sách lịch hẹn của bạn:");
                                foreach (var appt in appointments)
                                {
                                    string statusStr = appt.Status switch
                                    {
                                        AppointmentStatus.Pending => "Đang chờ chủ trọ duyệt",
                                        AppointmentStatus.Confirmed => "Đã xác nhận thành công",
                                        AppointmentStatus.Rejected => "Đã bị từ chối",
                                        AppointmentStatus.Cancelled => "Đã hủy",
                                        _ => "Không xác định"
                                    };
                                    sb.AppendLine($"- Lịch hẹn ngày: {appt.AppointmentDateTime:dd/MM/yyyy HH:mm}");
                                    sb.AppendLine($"  Phòng trọ hẹn xem: {appt.Property?.PropertyName}");
                                    sb.AppendLine($"  Địa chỉ: {appt.Property?.Address}");
                                    sb.AppendLine($"  Trạng thái: {statusStr}");
                                    if (!string.IsNullOrEmpty(appt.Note)) sb.AppendLine($"  Ghi chú: {appt.Note}");
                                }
                            }
                            else
                            {
                                sb.AppendLine("\nBạn chưa có lịch hẹn nào trên hệ thống.");
                            }

                            // 3. Lấy danh sách khiếu nại của Tenant
                            var complaints = await _dbContext.Complaints
                                .Where(com => com.CreatorId == userId)
                                .OrderByDescending(com => com.CreatedAt)
                                .Take(5)
                                .ToListAsync();

                            if (complaints.Any())
                            {
                                sb.AppendLine("\nDanh sách khiếu nại/yêu cầu sửa chữa đã gửi:");
                                foreach (var comp in complaints)
                                {
                                    string statusStr = comp.Status switch
                                    {
                                        ComplaintStatus.Pending => "Đang chờ xử lý",
                                        ComplaintStatus.Processing => "Đang tiến hành xử lý",
                                        ComplaintStatus.Resolved => "Đã giải quyết xong",
                                        _ => "Đang xử lý"
                                    };
                                    string typeStr = comp.RelatedType switch
                                    {
                                        ComplaintRelatedType.Property => "Cơ sở vật chất phòng trọ",
                                        ComplaintRelatedType.Invoice => "Hóa đơn thanh toán",
                                        ComplaintRelatedType.Contract => "Hợp đồng thuê",
                                        _ => "Khác"
                                    };
                                    sb.AppendLine($"- Ngày gửi: {comp.CreatedAt:dd/MM/yyyy}");
                                    sb.AppendLine($"  Tiêu đề: {comp.Title}");
                                    sb.AppendLine($"  Nội dung phản ánh: {comp.Content}");
                                    sb.AppendLine($"  Trạng thái: {statusStr}");
                                    if (!string.IsNullOrEmpty(comp.AdminResponse)) sb.AppendLine($"  Chủ trọ phản hồi: {comp.AdminResponse}");
                                    if (comp.ResolvedAt.HasValue) sb.AppendLine($"  Ngày giải quyết: {comp.ResolvedAt:dd/MM/yyyy}");
                                }
                            }
                        }
                        else
                        {
                            // 1. Lấy thông tin các phòng trọ do chủ nhà quản lý (bỏ phòng bị từ chối)
                            var properties = await _dbContext.Properties
                                .Where(p => p.LandlordId == userId && p.ModerationStatus != ModerationStatusEnum.Rejected)
                                .ToListAsync();

                            if (properties.Any())
                            {
                                sb.AppendLine($"\nSố lượng phòng quản lý: {properties.Count}");
                                sb.AppendLine("Danh sách các phòng trọ quản lý:");
                                foreach (var p in properties)
                                {
                                    sb.AppendLine($"- Phòng: {p.PropertyName}");
                                    sb.AppendLine($"  Địa chỉ: {p.Address}");
                                    sb.AppendLine($"  Giá thuê: {p.Price:N0} VND/tháng");
                                    
                                    string statusText;
                                    if (p.ModerationStatus == ModerationStatusEnum.Pending)
                                    {
                                        statusText = "Chờ duyệt";
                                    }
                                    else
                                    {
                                        statusText = p.AvailabilityStatus switch
                                        {
                                            AvailabilityStatusEnum.Available => "Trống (Sẵn sàng cho thuê)",
                                            AvailabilityStatusEnum.Rented => "Đã cho thuê",
                                            AvailabilityStatusEnum.Maintenance => "Đang sửa chữa",
                                            _ => "Không xác định"
                                        };
                                    }
                                    
                                    sb.AppendLine($"  Trạng thái phòng: {statusText}");
                                    sb.AppendLine($"  Trạng thái duyệt: {(p.ModerationStatus == ModerationStatusEnum.Approved ? "Đã duyệt" : "Đang chờ duyệt")}");
                                    
                                    // Tìm hợp đồng đang chạy của phòng này (hoạt động hoặc sắp hết hạn)
                                    var c = await _dbContext.Contracts
                                        .Include(con => con.Tenant)
                                        .FirstOrDefaultAsync(con => con.PropertyId == p.Id && (con.Status == ContractStatus.Active || con.Status == ContractStatus.NearExpiry));

                                    if (c != null)
                                    {
                                        sb.AppendLine($"  Đang được thuê bởi: {c.Tenant?.FullName} ({c.Tenant?.PhoneNumber})");
                                        sb.AppendLine($"  Thời hạn: {c.StartDate:dd/MM/yyyy} - {c.EndDate:dd/MM/yyyy}");
                                    }
                                }

                                var propertyIds = properties.Select(pr => pr.Id).ToList();

                                // 2. Lấy danh sách hóa đơn chưa được thanh toán (tính cả hợp đồng đang Active hoặc NearExpiry)
                                var unpaidInvoices = await _dbContext.Invoices
                                    .Include(i => i.Contract)
                                    .ThenInclude(c => c.Tenant)
                                    .Include(i => i.Contract)
                                    .ThenInclude(c => c.Property)
                                    .Where(i => propertyIds.Contains(i.Contract.PropertyId) && i.Status != InvoiceStatus.Paid && (i.Contract.Status == ContractStatus.Active || i.Contract.Status == ContractStatus.NearExpiry))
                                    .ToListAsync();

                                if (unpaidInvoices.Any())
                                {
                                    sb.AppendLine("\nDanh sách hóa đơn của khách thuê chưa thanh toán xong:");
                                    foreach (var inv in unpaidInvoices)
                                    {
                                        sb.AppendLine($"- Hóa đơn kì {inv.Period:MM/yyyy} của phòng {inv.Contract.Property?.PropertyName}");
                                        sb.AppendLine($"  Khách thuê: {inv.Contract.Tenant?.FullName} ({inv.Contract.Tenant?.PhoneNumber})");
                                        sb.AppendLine($"  Tổng tiền: {inv.Total:N0} VND");
                                        sb.AppendLine($"  Hạn thanh toán: {inv.DueDate:dd/MM/yyyy}");
                                        sb.AppendLine($"  Trạng thái: {(inv.Status == InvoiceStatus.Partial ? "Đã đóng một phần" : "Chưa thanh toán")}");
                                    }
                                }

                                // 3. Lấy lịch xem phòng sắp tới của Landlord
                                var landlordAppointments = await _dbContext.Appointments
                                    .Include(a => a.Property)
                                    .Include(a => a.User)
                                    .Where(a => propertyIds.Contains(a.PropertyId))
                                    .OrderBy(a => a.AppointmentDateTime)
                                    .ToListAsync();

                                if (landlordAppointments.Any())
                                {
                                    sb.AppendLine("\nDanh sách các lịch hẹn xem phòng của khách hàng:");
                                    foreach (var appt in landlordAppointments)
                                    {
                                        string statusStr = appt.Status switch
                                        {
                                            AppointmentStatus.Pending => "Đang chờ bạn duyệt",
                                            AppointmentStatus.Confirmed => "Đã xác nhận",
                                            AppointmentStatus.Rejected => "Đã từ chối",
                                            AppointmentStatus.Cancelled => "Khách đã hủy",
                                            _ => "Không xác định"
                                        };
                                        sb.AppendLine($"- Lịch xem ngày: {appt.AppointmentDateTime:dd/MM/yyyy HH:mm}");
                                        sb.AppendLine($"  Phòng trọ: {appt.Property?.PropertyName}");
                                        sb.AppendLine($"  Khách hẹn: {appt.User?.FullName} ({appt.User?.PhoneNumber})");
                                        sb.AppendLine($"  Trạng thái: {statusStr}");
                                        if (!string.IsNullOrEmpty(appt.Note)) sb.AppendLine($"  Ghi chú: {appt.Note}");
                                    }
                                }

                                // 4. Lấy phàn nàn/yêu cầu sửa chữa của khách thuê trọ thuộc quản lý của Landlord (tài sản, hợp đồng, hóa đơn)
                                var landlordComplaints = await _dbContext.Complaints
                                    .Include(com => com.Creator)
                                    .Where(com => 
                                        (com.RelatedType == ComplaintRelatedType.Property && propertyIds.Contains(com.RelatedId)) ||
                                        (com.RelatedType == ComplaintRelatedType.Contract && _dbContext.Contracts.Any(con => con.Id == com.RelatedId && propertyIds.Contains(con.PropertyId))) ||
                                        (com.RelatedType == ComplaintRelatedType.Invoice && _dbContext.Invoices.Any(inv => inv.Id == com.RelatedId && propertyIds.Contains(inv.Contract.PropertyId)))
                                    )
                                    .OrderByDescending(com => com.CreatedAt)
                                    .Take(10)
                                    .ToListAsync();

                                if (landlordComplaints.Any())
                                {
                                    sb.AppendLine("\nDanh sách phàn nàn/yêu cầu sửa chữa của khách thuê trọ:");
                                    foreach (var comp in landlordComplaints)
                                    {
                                        string statusStr = comp.Status switch
                                        {
                                            ComplaintStatus.Pending => "Chưa xử lý (Cần xử lý gấp)",
                                            ComplaintStatus.Processing => "Đang tiến hành xử lý",
                                            ComplaintStatus.Resolved => "Đã giải quyết xong",
                                            _ => "Chưa xử lý"
                                        };
                                        sb.AppendLine($"- Ngày gửi: {comp.CreatedAt:dd/MM/yyyy}");
                                        sb.AppendLine($"  Người gửi: {comp.Creator?.FullName} ({comp.Creator?.PhoneNumber})");
                                        sb.AppendLine($"  Tiêu đề: {comp.Title}");
                                        sb.AppendLine($"  Nội dung phản ánh: {comp.Content}");
                                        sb.AppendLine($"  Trạng thái: {statusStr}");
                                        if (!string.IsNullOrEmpty(comp.AdminResponse)) sb.AppendLine($"  Bạn đã phản hồi: {comp.AdminResponse}");
                                    }
                                }
                            }
                        }

                        contextString = sb.ToString();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi lấy dữ liệu ngữ cảnh RAG cho chatbot");
                }
            }

            try
            {
                // Gọi Python microservice
                var payload = new
                {
                    message = request.Message,
                    history = request.History.Select(h => new { role = h.Role, text = h.Text }),
                    user_role = request.UserRole,
                    context = contextString
                };

                var json = JsonSerializer.Serialize(payload, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_aiServiceBaseUrl}/chat", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<AiChatResponse>(responseBody, _jsonOptions);
                    if (result != null)
                        return MapToChatResponse(result);
                }
                else
                {
                    _logger.LogWarning("AI service trả về status {Status}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi AI service endpoint /chat");
            }

            // Fallback response khi AI service không khả dụng
            return new ChatResponse
            {
                Reply = "Xin chào! Tôi là trợ lý hỗ trợ của hệ thống quản lý phòng trọ. Hiện tại hệ thống AI đang bảo trì. Vui lòng liên hệ trực tiếp với chủ trọ.",
                Emotion = new EmotionResult { Label = "neutral", LabelVi = "bình thường", Urgency = "low" },
                Suggestions = new List<string> { "Hỏi về hóa đơn", "Xem hợp đồng", "Gửi khiếu nại" },
                Prompt = "Trợ lý ảo hệ thống Quản lý phòng trọ (AI Offline Fallback Mode)",
                Context = contextString,
            };
        }

        public async Task<EmotionResult> AnalyzeEmotionAsync(string text)
        {
            try
            {
                var payload = new { text };
                var json = JsonSerializer.Serialize(payload, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_aiServiceBaseUrl}/analyze-emotion", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<EmotionResult>(responseBody, _jsonOptions);
                    if (result != null) return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi AI service endpoint /analyze-emotion");
            }

            return new EmotionResult { Label = "neutral", LabelVi = "bình thường", Urgency = "low" };
        }

        public async Task<ComplaintAnalysisResponse> AnalyzeComplaintAsync(ChatComplaintRequest request)
        {
            try
            {
                var payload = new
                {
                    content = request.Content,
                    tenant_id = request.TenantId,
                    tenant_name = request.TenantName,
                };

                var json = JsonSerializer.Serialize(payload, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_aiServiceBaseUrl}/complaint", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ComplaintAnalysisResponse>(responseBody, _jsonOptions);
                    if (result != null) return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi AI service endpoint /complaint");
            }

            return new ComplaintAnalysisResponse
            {
                Emotion = new EmotionResult { Label = "neutral", Urgency = "low" },
                Urgency = "low",
                SuggestedCategory = "general_complaint",
                SuggestedResponse = "Khiếu nại của bạn đã được tiếp nhận. Chúng tôi sẽ phản hồi sớm nhất.",
            };
        }

        // Private Helpers

        private static ChatResponse MapToChatResponse(AiChatResponse ai) => new()
        {
            Reply = ai.Reply ?? string.Empty,
            Emotion = new EmotionResult
            {
                Label = ai.Emotion?.Label ?? "neutral",
                LabelVi = ai.Emotion?.LabelVi ?? "bình thường",
                Score = ai.Emotion?.Score ?? 0,
                Urgency = ai.Emotion?.Urgency ?? "low",
                Source = ai.Emotion?.Source ?? "unknown",
                Model = ai.Emotion?.Model ?? "unknown",
                Color = ai.Emotion?.Color ?? "#94a3b8",
                AllScores = ai.Emotion?.AllScores ?? new(),
            },
            Suggestions = ai.Suggestions ?? new(),
            Prompt = ai.Prompt ?? string.Empty,
            Context = ai.Context ?? string.Empty,
        };

        // Internal types for deserializing Python responses
        private class AiChatResponse
        {
            [JsonPropertyName("reply")]
            public string? Reply { get; set; }
            [JsonPropertyName("emotion")]
            public EmotionResult? Emotion { get; set; }
            [JsonPropertyName("suggestions")]
            public List<string>? Suggestions { get; set; }
            [JsonPropertyName("prompt")]
            public string? Prompt { get; set; }
            [JsonPropertyName("context")]
            public string? Context { get; set; }
        }
    }
}
