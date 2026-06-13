using AutoMapper;
using Backend_Boarding_house_management_system.DTOs.Rating.Requests;
using Backend_Boarding_house_management_system.DTOs.Rating.Responses;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Page;
using Plainquire.Sort;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class RatingService : IRatingService
    {
        private readonly AppDbContext _context;
        private readonly IRatingRepository _ratingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAspectAnalysisService _aspectAnalysisService;

        public RatingService(
            AppDbContext context,
            IRatingRepository ratingRepository,
            IUserRepository userRepository,
            IPropertyRepository propertyRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IAspectAnalysisService aspectAnalysisService)
        {
            _context = context;
            _ratingRepository = ratingRepository;
            _userRepository = userRepository;
            _propertyRepository = propertyRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _aspectAnalysisService = aspectAnalysisService;
        }

        public async Task<RatingResponse> GetRatingByIdAsync(GetRatingByIdRequest request)
        {
            var rating = await _ratingRepository.GetByIdAsync(request.Id);
            if (rating == null)
                throw new NotFoundException($"Khong tim thay danh gia voi Id '{request.Id}'.");

            return _mapper.Map<RatingResponse>(rating);
        }

        public async Task<RatingDetailResponse> GetRatingDetailByIdAsync(GetRatingByIdRequest request)
        {
            var rating = await _ratingRepository.GetByIdWithDetailsAsync(request.Id);
            if (rating == null)
                throw new NotFoundException($"Khong tim thay danh gia voi Id '{request.Id}'.");

            return _mapper.Map<RatingDetailResponse>(rating);
        }

        public async Task<RatingListResponse> GetRatingsByFilterAsync(
            EntityFilter<Rating> filter,
            EntitySort<Rating> sort,
            EntityPage page)
        {
            var (ratings, totalCount) = await _ratingRepository.GetByFilterAsync(filter, sort, page);
            return new RatingListResponse
            {
                Items = _mapper.Map<List<RatingResponse>>(ratings),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
        }

        public async Task<RatingResponse> CreateRatingAsync(CreateRatingRequest request)
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(request.TenantId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban chi duoc danh gia voi tai khoan cua minh.");

            var tenant = await _userRepository.GetByIdAsync(request.TenantId);
            if (tenant == null)
                throw new NotFoundException($"Khong tim thay tenant voi Id '{request.TenantId}'.");

            if (!string.Equals(tenant.Role, "Tenant", StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("User duoc chon khong phai tenant.");

            if (!await _propertyRepository.ExistsAsync(request.PropertyId))
                throw new NotFoundException($"Khong tim thay bat dong san voi Id '{request.PropertyId}'.");

            if (!await _context.Contracts.AnyAsync(c => c.TenantId == request.TenantId && c.PropertyId == request.PropertyId))
                throw new BadRequestException("Ban chua tung co hop dong voi bat dong san nay.");

            if (await _ratingRepository.ExistsByTenantAndPropertyAsync(request.TenantId, request.PropertyId))
                throw new ConflictException("Tenant nay da danh gia bat dong san nay roi.");

            var rating = _mapper.Map<Rating>(request);
            rating.Id = Guid.NewGuid().ToString();
            rating.CreatedAt = DateTime.UtcNow;

            // Server-side ABSA (tối ưu luồng):
            // - Gọi mô hình Two-Head (Python FastAPI) để phân tích review thành các (Aspect, Sentiment, Confidence).
            // - Tạo RatingAspect entities.
            // - Trong transaction: lưu Rating + RatingAspects → RecalculatePropertyAspectScores (cập nhật counts + WeightedScore).
            // - Luôn có fallback keyword nếu model service chậm/lỗi (đảm bảo không block tạo rating).
            // - Phối hợp: dữ liệu này sau đó được dùng trong recommendation (xem PropertyService).
            var analyzed = await _aspectAnalysisService.AnalyzeReviewAspectsAsync(request.Content, request.Stars);
            var ratingAspects = analyzed.Select(a => new RatingAspect
            {
                Id = Guid.NewGuid().ToString(),
                RatingId = rating.Id,
                Aspect = a.Aspect,
                Sentiment = a.Sentiment,
                Confidence = a.Confidence,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await _ratingRepository.AddAsync(rating);

                if (ratingAspects.Count > 0)
                {
                    _context.RatingAspects.AddRange(ratingAspects);
                    await _context.SaveChangesAsync();
                    await RecalculatePropertyAspectScoresAsync(request.PropertyId);
                }

                // Update landlord reputation (+1 on new rating)
                var property = await _propertyRepository.GetByIdAsync(request.PropertyId);
                if (property != null)
                {
                    var landlord = await _userRepository.GetByIdAsync(property.LandlordId);
                    if (landlord != null)
                    {
                        landlord.ReputationScore += 1;
                        await _userRepository.UpdateAsync(landlord);
                    }

                    // Auto notify landlord
                    var notif = new Notification
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = property.LandlordId,
                        Type = "Rating",
                        Content = "Ban co danh gia moi.",
                        IsRead = false,
                        Timestamp = DateTime.UtcNow,
                        RelatedId = rating.Id
                    };
                    _context.Notifications.Add(notif);
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            return _mapper.Map<RatingResponse>(rating);
        }

        public async Task<bool> UpdateRatingAsync(UpdateRatingRequest request)
        {
            var rating = await _ratingRepository.GetByIdAsync(request.Id);
            if (rating == null)
                throw new NotFoundException($"Khong tim thay danh gia voi Id '{request.Id}'.");

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(rating.TenantId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen cap nhat danh gia nay.");

            _mapper.Map(request, rating);
            await _ratingRepository.UpdateAsync(rating);
            return true;
        }

        public async Task<bool> DeleteRatingAsync(DeleteRatingRequest request)
        {
            var rating = await _ratingRepository.GetByIdAsync(request.Id);
            if (rating == null)
                throw new NotFoundException($"Khong tim thay danh gia voi Id '{request.Id}'.");

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(rating.TenantId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen xoa danh gia nay.");

            await _ratingRepository.DeleteAsync(request.Id);

            // Adjust landlord reputation (-1 on delete)
            var property = await _propertyRepository.GetByIdAsync(rating.PropertyId);
            if (property != null)
            {
                var landlord = await _userRepository.GetByIdAsync(property.LandlordId);
                if (landlord != null)
                {
                    landlord.ReputationScore = Math.Max(0, landlord.ReputationScore - 1);
                    await _userRepository.UpdateAsync(landlord);
                }
            }

            await RecalculatePropertyAspectScoresAsync(rating.PropertyId);
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task RecalculatePropertyAspectScoresAsync(string propertyId)
        {
            if (string.IsNullOrWhiteSpace(propertyId)) return;

            var aspectGroups = await _context.RatingAspects
                .Where(ra => ra.Rating.PropertyId == propertyId)
                .AsNoTracking()
                .GroupBy(ra => ra.Aspect)
                .Select(g => new
                {
                    Aspect = g.Key,
                    Pos = g.Count(x => x.Sentiment == RatingAttitude.Positive),
                    Neg = g.Count(x => x.Sentiment == RatingAttitude.Negative),
                    Neu = g.Count(x => x.Sentiment == RatingAttitude.Neutral)
                })
                .ToListAsync();

            var existingScores = await _context.PropertyAspectScores
                .Where(s => s.PropertyId == propertyId)
                .ToListAsync();

            var scoreDict = existingScores.ToDictionary(s => s.Aspect);
            var touched = new HashSet<ReviewAspect>();

            foreach (var g in aspectGroups)
            {
                touched.Add(g.Aspect);
                int total = g.Pos + g.Neg + g.Neu;
                decimal wScore = 0m;
                if (total > 0)
                {
                    double raw = (g.Pos - g.Neg) / (double)total;
                    wScore = Math.Round((decimal)((raw + 1) / 2 * 100), 2);
                }

                if (scoreDict.TryGetValue(g.Aspect, out var ent))
                {
                    ent.PositiveCount = g.Pos;
                    ent.NegativeCount = g.Neg;
                    ent.NeutralCount = g.Neu;
                    ent.TotalCount = total;
                    ent.WeightedScore = wScore;
                    ent.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    var newEnt = new PropertyAspectScore
                    {
                        Id = Guid.NewGuid().ToString(),
                        PropertyId = propertyId,
                        Aspect = g.Aspect,
                        PositiveCount = g.Pos,
                        NegativeCount = g.Neg,
                        NeutralCount = g.Neu,
                        TotalCount = total,
                        WeightedScore = wScore,
                        LastUpdated = DateTime.UtcNow
                    };
                    _context.PropertyAspectScores.Add(newEnt);
                }
            }

            foreach (var kvp in scoreDict.Where(k => !touched.Contains(k.Key)).ToList())
            {
                _context.PropertyAspectScores.Remove(kvp.Value);
            }
        }

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private bool IsCurrentUserAdmin()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return false;
            return user.IsInRole("Admin") ||
                   string.Equals(user.FindFirstValue(ClaimTypes.Role), "Admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
