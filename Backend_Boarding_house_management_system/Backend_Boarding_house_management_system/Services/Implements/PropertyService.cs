using Backend_Boarding_house_management_system.DTOs.Property.Requests;
using Backend_Boarding_house_management_system.DTOs.Property.Responses;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using Backend_Boarding_house_management_system.Options;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class PropertyService : IPropertyService
    {
        private readonly AppDbContext _context;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAreaRepository _areaRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // For personalized recommendation from history (injected; already registered in DI)
        private readonly IViewHistoryRepository _viewHistoryRepository;
        private readonly ISearchHistoryRepository _searchHistoryRepository;
        private readonly IPhotoService _photoService;
        private readonly IPropertyScorer _propertyScorer;

        public PropertyService(
            AppDbContext context,
            IPropertyRepository propertyRepository,
            IUserRepository userRepository,
            IAreaRepository areaRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IViewHistoryRepository viewHistoryRepository,
            ISearchHistoryRepository searchHistoryRepository,
            IPhotoService photoService,
            IPropertyScorer propertyScorer)
        {
            _context = context;
            _propertyRepository = propertyRepository;
            _userRepository = userRepository;
            _areaRepository = areaRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _viewHistoryRepository = viewHistoryRepository;
            _searchHistoryRepository = searchHistoryRepository;
            _photoService = photoService;
            _propertyScorer = propertyScorer;
        }

        public async Task<PropertyResponse> GetPropertyByIdAsync(GetPropertyByIdRequest request)
        {
            var property = await _propertyRepository.GetByIdAsync(request.Id);
            if (property == null)
                throw new NotFoundException("Khong tim thay bat dong san.");

            return _mapper.Map<PropertyResponse>(property);
        }

        public async Task<PropertyDetailResponse> GetPropertyDetailByIdAsync(GetPropertyByIdRequest request)
        {
            var property = await _propertyRepository.GetByIdWithDetailsAsync(request.Id);
            if (property == null)
                throw new NotFoundException("Khong tim thay bat dong san.");

            // Auto ghi nhận ViewHistory cho user đã đăng nhập (tín hiệu mạnh cho recommendation)
            await LogViewHistoryIfAuthenticatedAsync(request.Id);

            return _mapper.Map<PropertyDetailResponse>(property);
        }

        public async Task<PropertyListResponse> GetModerationPropertiesAsync(GetModerationPropertiesRequest request)
        {
            var status = ParseModerationStatus(request.Status, ModerationStatusEnum.Pending);
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var query = _context.Properties
                .AsNoTracking()
                .Where(property => property.ModerationStatus == status)
                .OrderByDescending(property => property.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PropertyListResponse
            {
                Items = _mapper.Map<List<PropertyResponse>>(items),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PropertyListResponse> GetPropertiesByFilterAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page,
            IDictionary<ReviewAspect, double>? searchAspectBoosts = null)
        {
            // Giữ nguyên hành vi cũ (có thể personalize nếu có user) + hỗ trợ aspect boost từ search
            return await GetPropertiesInternalAsync(filter, sort, page, personalizeIfPossible: true, searchAspectBoosts);
        }

        public async Task<PropertyListResponse> GetRecommendedPropertiesAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page,
            IDictionary<ReviewAspect, double>? searchAspectBoosts = null)
        {
            // Dedicated endpoint for explicit "đề cử" – luôn cố gắng personalize (mặc định PersonalMatch) + aspect boost từ search hiện tại
            return await GetPropertiesInternalAsync(filter, sort, page, personalizeIfPossible: true, searchAspectBoosts);
        }

        public async Task<PropertyListResponse> GetMostViewedPropertiesAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page,
            IDictionary<ReviewAspect, double>? searchAspectBoosts = null)
        {
            // Lấy view counts toàn cục (giới hạn candidates)
            var viewCounts = await _propertyRepository.GetPropertyViewCountsAsync(filter, maxCandidates: 300);

            if (viewCounts.Count == 0)
            {
                // Fallback: trả về list thường nếu chưa có view history
                return await GetPropertiesByFilterAsync(filter, sort, page, searchAspectBoosts);
            }

            // Lấy candidates (giới hạn) rồi sort theo view count desc + secondary CreatedAt
            var candidates = await _propertyRepository.GetFilteredCandidatesForRecAsync(filter, maxCandidates: 300);

            var scored = candidates
                .Select(p => new
                {
                    Property = p,
                    ViewCount = viewCounts.TryGetValue(p.Id, out var c) ? c : 0,
                    Score = _propertyScorer.CalculateScore(
                        p,
                        preference: null,
                        userPositiveAspects: null,
                        userNegativeAspects: null,
                        mode: RecommendationMode.HighAspectQuality,   // MostViewed ưu tiên chất lượng aspect cao
                        searchAspectBoosts: searchAspectBoosts,       // hỗ trợ boost theo aspect user đang search
                        userId: null)
                })
                .OrderByDescending(x => x.ViewCount * 1.0 + x.Score * 0.6)
                .ThenByDescending(x => x.Property.CreatedAt)
                .ToList();

            var pageNumber = (int)(page.PageNumber ?? 1);
            var pageSize = (int)(page.PageSize ?? 10);
            var skip = Math.Max(0, (pageNumber - 1) * pageSize);

            var finalItems = scored.Skip(skip).Take(pageSize).Select(x => x.Property).ToList();

            // TotalCount giữ nguyên theo filter (như personalized)
            var countQuery = _context.Properties.AsNoTracking().Where(filter);
            var totalCount = await countQuery.CountAsync();

            return new PropertyListResponse
            {
                Items = _mapper.Map<List<PropertyResponse>>(finalItems),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PropertyListResponse> GetTrendingPropertiesAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page,
            IDictionary<ReviewAspect, double>? searchAspectBoosts = null)
        {
            // 1. Lấy global recent searches (không per-user) để trích xuất trending terms.
            // (Sử dụng direct context vì repo hiện tại tập trung vào per-user recent.)
            var globalSearches = await _context.SearchHistories
                .AsNoTracking()
                .OrderByDescending(sh => sh.Timestamp)
                .Take(500)
                .ToListAsync();

            // Parse để thu thập popular signals (district, price band, amenity)
            var popularDistricts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var popularAmenityIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            decimal? popPriceMin = null, popPriceMax = null;

            foreach (var sh in globalSearches)
            {
                if (string.IsNullOrWhiteSpace(sh.Filters)) continue;
                try
                {
                    using var doc = JsonDocument.Parse(sh.Filters);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("district", out var d) && d.ValueKind == JsonValueKind.String)
                    {
                        var val = d.GetString();
                        if (!string.IsNullOrEmpty(val)) popularDistricts.Add(val);
                    }
                    // areaId cũng được chấp nhận
                    if (root.TryGetProperty("areaId", out var a) && a.ValueKind == JsonValueKind.String)
                    {
                        var val = a.GetString();
                        if (!string.IsNullOrEmpty(val)) popularDistricts.Add(val); // treat as area proxy
                    }

                    if (root.TryGetProperty("priceMin", out var pmin) && pmin.TryGetDecimal(out var pminVal))
                        popPriceMin = popPriceMin.HasValue ? Math.Min(popPriceMin.Value, pminVal) : pminVal;
                    if (root.TryGetProperty("priceMax", out var pmax) && pmax.TryGetDecimal(out var pmaxVal))
                        popPriceMax = popPriceMax.HasValue ? Math.Max(popPriceMax.Value, pmaxVal) : pmaxVal;

                    if (root.TryGetProperty("amenities", out var amArr) && amArr.ValueKind == JsonValueKind.Array ||
                        root.TryGetProperty("amenityIds", out amArr) && amArr.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var el in amArr.EnumerateArray())
                        {
                            if (el.ValueKind == JsonValueKind.String)
                            {
                                var aid = el.GetString();
                                if (!string.IsNullOrEmpty(aid)) popularAmenityIds.Add(aid);
                            }
                        }
                    }
                }
                catch { /* ignore bad json */ }
            }

            // 2. Lấy view counts (global popularity boost)
            var viewCounts = await _propertyRepository.GetPropertyViewCountsAsync(filter, maxCandidates: 300);

            // 3. Lấy candidates và score theo trending signals (global)
            var candidates = await _propertyRepository.GetFilteredCandidatesForRecAsync(filter, maxCandidates: 300);

            var scored = candidates.Select(p =>
            {
                double score = 5;

                // Trending district / area match (từ searches)
                if (!string.IsNullOrEmpty(p.AreaId) && popularDistricts.Contains(p.AreaId))
                    score += 35;

                // Price fit theo popular search ranges
                if ((popPriceMin.HasValue && popPriceMax.HasValue) || (popPriceMin.HasValue || popPriceMax.HasValue))
                {
                    decimal target = (popPriceMin ?? 0) + ((popPriceMax ?? (popPriceMin ?? 0) + 5_000_000) - (popPriceMin ?? 0)) / 2;
                    if (popPriceMin.HasValue && popPriceMax.HasValue)
                        target = (popPriceMin.Value + popPriceMax.Value) / 2;
                    decimal distance = Math.Abs(p.Price - target);
                    decimal tolerance = Math.Max(500_000, target * 0.3m);
                    double fit = Math.Max(0, 1 - (double)(distance / tolerance));
                    score += fit * 25;
                }

                // Amenity trending
                int amenityHits = 0;
                foreach (var ra in p.RoomAmenities)
                {
                    if (string.Equals(ra.Status, "Working", StringComparison.OrdinalIgnoreCase) &&
                        popularAmenityIds.Contains(ra.AmenityId))
                        amenityHits++;
                }
                if (amenityHits > 0) score += Math.Min(amenityHits * 6, 20);

                // View count boost (most viewed cũng là tín hiệu trending)
                if (viewCounts.TryGetValue(p.Id, out var vc))
                    score += Math.Min(vc * 1.5, 30);

                // === Sử dụng Scoring Engine (HighAspectQuality + global bias) + search aspect boosts ===
                double aspectScore = _propertyScorer.CalculateScore(
                    p,
                    preference: null,
                    userPositiveAspects: null,
                    userNegativeAspects: null,
                    mode: RecommendationMode.HighAspectQuality,
                    searchAspectBoosts: searchAspectBoosts,
                    userId: null);

                score += (aspectScore - 10) * 0.75;   // bias global aspect chất lượng cao cho trending

                return new { Property = p, Score = score };
            })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Property.CreatedAt)
            .ToList();

            var pageNumber = (int)(page.PageNumber ?? 1);
            var pageSize = (int)(page.PageSize ?? 10);
            var skip = Math.Max(0, (pageNumber - 1) * pageSize);

            var finalItems = scored.Skip(skip).Take(pageSize).Select(x => x.Property).ToList();

            var countQuery = _context.Properties.AsNoTracking().Where(filter);
            var totalCount = await countQuery.CountAsync();

            return new PropertyListResponse
            {
                Items = _mapper.Map<List<PropertyResponse>>(finalItems),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PopularPriceRangesResponse> GetPopularPriceRangesAsync()
        {
            // Định nghĩa các bucket phổ biến (có thể sau này đưa ra config)
            var buckets = new List<(string Label, decimal Min, decimal Max)>
            {
                ("Dưới 3 triệu", 0, 3_000_000),
                ("3 - 5 triệu", 3_000_000, 5_000_000),
                ("5 - 7 triệu", 5_000_000, 7_000_000),
                ("7 - 10 triệu", 7_000_000, 10_000_000),
                ("Trên 10 triệu", 10_000_000, decimal.MaxValue)
            };

            // Filter chỉ các phòng đang sẵn sàng
            var filter = new EntityFilter<Property>(); // Plainquire filter rỗng, ta apply status trong repo
            // Để đơn giản, truyền filter trống và để repo xử lý status
            var bucketCounts = await _propertyRepository.GetPriceBucketCountsAsync(filter, buckets);

            var ranges = new List<PopularPriceRange>();
            int total = 0;
            foreach (var (label, min, max) in buckets)
            {
                var count = bucketCounts.TryGetValue(label, out var c) ? c : 0;
                ranges.Add(new PopularPriceRange
                {
                    Label = label,
                    Min = min,
                    Max = max == decimal.MaxValue ? 0 : max, // 0 nghĩa là "trở lên"
                    Count = count
                });
                total += count;
            }

            return new PopularPriceRangesResponse
            {
                Ranges = ranges,
                TotalPropertiesConsidered = total
            };
        }

        /// <summary>
        /// Internal dùng chung cho list thông thường và recommended.
        /// Khi có user đăng nhập + personalizeIfPossible=true: lấy candidate, tính score từ View/SearchHistory, re-rank trước khi page.
        /// </summary>
        private async Task<PropertyListResponse> GetPropertiesInternalAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page,
            bool personalizeIfPossible,
            IDictionary<ReviewAspect, double>? searchAspectBoosts = null)
        {
            var userId = GetCurrentUserId();

            // Tính total theo filter (không bị ảnh hưởng bởi re-rank)
            // Dùng count trực tiếp trên filter (tận dụng IQueryable của Plainquire)
            var countQuery = _context.Properties.AsNoTracking().Where(filter);
            var totalCount = await countQuery.CountAsync();

            List<Property> finalItems;

            if (!personalizeIfPossible || string.IsNullOrEmpty(userId))
            {
                // Giữ nguyên hành vi cũ hoàn toàn cho anonymous hoặc khi không personalize
                // Vẫn hỗ trợ searchAspectBoosts từ query hiện tại (boost theo aspect user vừa chọn)
                var (items, _) = await _propertyRepository.GetByFilterAsync(filter, sort, page);
                finalItems = items.ToList();

                // Nếu có aspect boosts từ search, re-rank nhẹ bằng scorer (không có lịch sử cá nhân)
                if (searchAspectBoosts != null && searchAspectBoosts.Any())
                {
                    var candidates = items.ToList();
                    var scoredAnonymous = candidates
                        .Select(p => new
                        {
                            Property = p,
                            Score = _propertyScorer.CalculateScore(
                                p,
                                preference: null,
                                userPositiveAspects: null,
                                userNegativeAspects: null,
                                mode: RecommendationMode.Balanced,
                                searchAspectBoosts: searchAspectBoosts,
                                userId: null)
                        })
                        .OrderByDescending(x => x.Score)
                        .ThenByDescending(x => x.Property.CreatedAt)
                        .Select(x => x.Property)
                        .ToList();

                    finalItems = scoredAnonymous;
                }
            }
            else
            {
                // Xây preference từ history (View + Search)
                var pref = await BuildUserPreferenceAsync(userId);

                // Lấy bounded candidates (kèm RoomAmenities + AspectScores để scoring)
                var candidates = (await _propertyRepository.GetFilteredCandidatesForRecAsync(filter, maxCandidates: 200)).ToList();

                // ABSA aspect interests: direct positive ratings (strong) + from view history (via pref.AspectInterests)
                HashSet<ReviewAspect> userPositiveAspects = new HashSet<ReviewAspect>(pref.AspectInterests);
                HashSet<ReviewAspect> userNegativeAspects = new HashSet<ReviewAspect>();

                try
                {
                    // Direct from user's ratings (strongest personal signal)
                    var userRatings = await _context.RatingAspects
                        .AsNoTracking()
                        .Where(ra => ra.Rating.TenantId == userId)
                        .Select(ra => new { ra.Aspect, ra.Sentiment })
                        .ToListAsync();

                    foreach (var r in userRatings)
                    {
                        if (r.Sentiment == RatingAttitude.Positive)
                            userPositiveAspects.Add(r.Aspect);
                        else if (r.Sentiment == RatingAttitude.Negative)
                            userNegativeAspects.Add(r.Aspect);
                    }
                }
                catch { /* non-fatal for recs */ }

                // Mặc định dùng PersonalMatch (có thể sau này nhận từ query param)
                var mode = RecommendationMode.PersonalMatch;

                // Tính điểm + re-rank bằng Scoring Engine mới (tách biệt, hỗ trợ nhiều mode)
                // searchAspectBoosts từ lần search hiện tại được truyền vào → property có điểm aspect cao theo user vừa chọn sẽ được đẩy lên
                var scored = candidates
                    .Select(p => new
                    {
                        Property = p,
                        Score = _propertyScorer.CalculateScore(p, pref, userPositiveAspects, userNegativeAspects, mode, searchAspectBoosts, userId)
                    })
                    .OrderByDescending(x => x.Score)
                    // Secondary: ưu tiên mới tạo nếu score ngang nhau (có thể mở rộng dùng sort nếu cần)
                    .ThenByDescending(x => x.Property.CreatedAt)
                    .ToList();

                // Áp dụng phân trang trên tập đã re-rank
                var pageNumber = (int)(page.PageNumber ?? 1);
                var pageSize = (int)(page.PageSize ?? 10);
                var skip = Math.Max(0, (pageNumber - 1) * pageSize);

                finalItems = scored
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(x => x.Property)
                    .ToList();
            }

            return new PropertyListResponse
            {
                Items = _mapper.Map<List<PropertyResponse>>(finalItems),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
        }

        public async Task<PropertyResponse> CreatePropertyAsync(CreatePropertyRequest request)
        {
            // Ownership: caller must be creating for themselves (or Admin)
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(request.LandlordId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban chi duoc tao bat dong san cho chinh minh.");

            var landlord = await _userRepository.GetByIdAsync(request.LandlordId);
            if (landlord == null)
                throw new NotFoundException($"Khong tim thay landlord voi Id '{request.LandlordId}'.");

            if (!string.Equals(landlord.Role, "Landlord", StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("User duoc chon khong phai landlord.");

            if (!string.IsNullOrWhiteSpace(request.AreaId))
            {
                var area = await _areaRepository.GetByIdAsync(request.AreaId);
                if (area == null)
                    throw new NotFoundException($"Khong tim thay khu vuc voi Id '{request.AreaId}'.");

                if (!string.Equals(area.LandlordId, request.LandlordId, StringComparison.Ordinal))
                    throw new BadRequestException("Khu vuc khong thuoc landlord da chon.");
            }

            var property = _mapper.Map<Property>(request);
            property.Id = Guid.NewGuid().ToString();
            property.CreatedAt = DateTime.UtcNow;
            property.ModerationStatus = ParseModerationStatus(request.ModerationStatus, ModerationStatusEnum.Pending);
            property.AvailabilityStatus = ParseAvailabilityStatus(request.Status, AvailabilityStatusEnum.Available);
            property.UpdatedAt = DateTime.UtcNow;

            await _propertyRepository.AddAsync(property);
            return _mapper.Map<PropertyResponse>(property);
        }

        public async Task<bool> UpdatePropertyAsync(UpdatePropertyRequest request)
        {
            var property = await _propertyRepository.GetByIdAsync(request.Id);
            if (property == null)
                throw new NotFoundException($"Khong tim thay bat dong san voi Id '{request.Id}'.");

            // Ownership check
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(property.LandlordId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen cap nhat bat dong san nay.");

            var requestedStatus = request.Status;
            var requestedModerationStatus = request.ModerationStatus;
            _mapper.Map(request, property);
            property.AvailabilityStatus = ParseAvailabilityStatus(requestedStatus, property.AvailabilityStatus);
            property.ModerationStatus = ParseModerationStatus(requestedModerationStatus, property.ModerationStatus);
            property.UpdatedAt = DateTime.UtcNow;
            await _propertyRepository.UpdateAsync(property);
            return true;
        }

        public async Task<bool> ApprovePropertyAsync(ApprovePropertyRequest request)
        {
            var property = await _propertyRepository.GetByIdAsync(request.PropertyId);
            if (property == null)
                throw new NotFoundException($"Khong tim thay bat dong san voi Id '{request.PropertyId}'.");

            // Ownership (Admin only typically, but enforce)
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(property.LandlordId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen duyet bat dong san nay.");

            if (property.ModerationStatus != ModerationStatusEnum.Pending)
                throw new BadRequestException("Chi co the duyet bat dong san dang trong trang Thai cho duyet.");

            property.ModerationStatus = ModerationStatusEnum.Approved;
            property.ApprovedAt = DateTime.UtcNow;
            property.UpdatedAt = DateTime.UtcNow;
            
            await _propertyRepository.UpdateAsync(property);
            return true;
        }

        public async Task<bool> RejectPropertyAsync(RejectPropertyRequest request)
        {
            var property = await _propertyRepository.GetByIdAsync(request.PropertyId);
            if (property == null)
                throw new NotFoundException($"Khong tim thay bat dong san voi Id '{request.PropertyId}'.");

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(property.LandlordId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen tu choi bat dong san nay.");

            if (property.ModerationStatus != ModerationStatusEnum.Pending)
                throw new BadRequestException("Chi co the tu choi bat dong san dang trong trang Thai cho duyet.");

            property.ModerationStatus = ModerationStatusEnum.Rejected;
            property.RejectionReason = request.RejectionReason;
            property.RejectedAt = DateTime.UtcNow;
            property.UpdatedAt = DateTime.UtcNow;
            
            await _propertyRepository.UpdateAsync(property);
            return true;
        }

        public async Task<bool> UpdateAvailabilityStatusAsync(UpdateAvailabilityStatusRequest request)
        {
            var property = await _propertyRepository.GetByIdAsync(request.PropertyId);
            if (property == null)
                throw new NotFoundException($"Khong tim thay bat dong san voi Id '{request.PropertyId}'.");

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(property.LandlordId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen cap nhat trang thai kha dung cho bat dong san nay.");

            if (property.ModerationStatus != ModerationStatusEnum.Approved)
                throw new BadRequestException("Chi co the cap nhat trang thai kha dung cho bat dong san da duoc duyet.");

            property.AvailabilityStatus = request.AvailabilityStatus;
            property.UpdatedAt = DateTime.UtcNow;
            
            await _propertyRepository.UpdateAsync(property);
            return true;
        }

        public async Task<bool> DeletePropertyAsync(DeletePropertyRequest request)
        {
            if (!await _propertyRepository.ExistsAsync(request.Id))
                throw new NotFoundException($"Khong tim thay bat dong san voi Id '{request.Id}'.");

            var property = await _propertyRepository.GetByIdAsync(request.Id);
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (property != null && !isAdmin && !string.Equals(property.LandlordId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen xoa bat dong san nay.");

            var blockers = await GetPropertyDeleteBlockersAsync(request.Id);
            if (blockers.Count > 0)
            {
                throw new ConflictException(
                    $"Khong the xoa bat dong san voi Id '{request.Id}' vi van con du lieu lien quan: {string.Join(", ", blockers)}.");
            }

            // Best-effort cleanup of images on Cloudinary to prevent orphaned files.
            // Must happen BEFORE DB delete (cascade will remove PropertyImage rows).
            // Parallelized for speed when property has many images.
            try
            {
                var images = await _context.PropertyImages
                    .AsNoTracking()
                    .Where(pi => pi.PropertyId == request.Id)
                    .ToListAsync();

                var deleteTasks = images
                    .Where(img => !string.IsNullOrWhiteSpace(img.PublicId))
                    .Select(async img =>
                    {
                        try
                        {
                            await _photoService.DeletePhotoAsync(img.PublicId);
                        }
                        catch
                        {
                            // Ignore per-image failures; we still want to allow property deletion.
                        }
                    });

                await Task.WhenAll(deleteTasks);
            }
            catch
            {
                // Ignore overall cleanup errors to not block deletion.
            }

            try
            {
                await _propertyRepository.DeleteAsync(request.Id);
            }
            catch (DbUpdateException)
            {
                throw new ConflictException(
                    $"Khong the xoa bat dong san voi Id '{request.Id}' vi van con du lieu lien quan trong he thong.");
            }

            return true;
        }

        private async Task<List<string>> GetPropertyDeleteBlockersAsync(string propertyId)
        {
            var blockers = new List<string>();

            if (await _context.Contracts.AnyAsync(x => x.PropertyId == propertyId))
                blockers.Add("hop dong");

            if (await _context.Appointments.AnyAsync(x => x.PropertyId == propertyId))
                blockers.Add("lich hen");

            if (await _context.Messages.AnyAsync(x => x.PropertyId == propertyId))
                blockers.Add("tin nhan");

            if (await _context.Ratings.AnyAsync(x => x.PropertyId == propertyId))
                blockers.Add("danh gia");

            if (await _context.RoomAmenities.AnyAsync(x => x.PropertyId == propertyId))
                blockers.Add("tien ich phong");

            return blockers;
        }

        private static ModerationStatusEnum ParseModerationStatus(string? value, ModerationStatusEnum fallback)
        {
            if (string.IsNullOrWhiteSpace(value)) return fallback;
            return Enum.TryParse<ModerationStatusEnum>(value, true, out var parsed)
                ? parsed
                : fallback;
        }

        private static AvailabilityStatusEnum ParseAvailabilityStatus(string? value, AvailabilityStatusEnum fallback)
        {
            if (string.IsNullOrWhiteSpace(value)) return fallback;

            if (Enum.TryParse<AvailabilityStatusEnum>(value, true, out var parsed))
            {
                return parsed;
            }

            return fallback;
        }

        // ==================== RECOMMENDATION HELPERS (ViewHistory + SearchHistory) ====================

        private async Task LogViewHistoryIfAuthenticatedAsync(string propertyId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            // Tránh spam: chỉ log nếu chưa xem property này trong 10 phút gần nhất
            var recentThreshold = DateTime.UtcNow.AddMinutes(-10);
            var hasRecent = await _context.ViewHistories
                .AsNoTracking()
                .AnyAsync(v => v.UserId == userId && v.PropertyId == propertyId && v.Timestamp >= recentThreshold);

            if (hasRecent)
                return;

            var view = new ViewHistory
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                PropertyId = propertyId,
                Timestamp = DateTime.UtcNow
            };

            await _context.ViewHistories.AddAsync(view);
            await _context.SaveChangesAsync();
        }

        // UserPreference record đã được di chuyển sang IPropertyScorer.cs (public)
        // để scorer và service dùng chung. (Hướng 1)

        private async Task<UserPreference> BuildUserPreferenceAsync(string userId)
        {
            var areaIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var amenityIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var prices = new List<decimal>();
            decimal? priceMin = null, priceMax = null;
            var aspectInterests = new HashSet<ReviewAspect>();

            // 1. Từ ViewHistory (tín hiệu mạnh: user đã thực sự xem chi tiết)
            //    Cũng phối hợp aspect scores từ các property đã xem (để infer sở thích aspect)
            var recentViews = await _viewHistoryRepository.GetRecentForUserAsync(userId, limit: 30);
            foreach (var vh in recentViews)
            {
                var p = vh.Property;
                if (p == null) continue;

                if (!string.IsNullOrEmpty(p.AreaId))
                    areaIds.Add(p.AreaId);

                prices.Add(p.Price);

                foreach (var ra in p.RoomAmenities)
                {
                    if (string.Equals(ra.Status, "Working", StringComparison.OrdinalIgnoreCase))
                        amenityIds.Add(ra.AmenityId);
                }

                // Aggregate high aspect scores from viewed properties (coordinates view history + ABSA data)
                if (p.PropertyAspectScores != null)
                {
                    foreach (var pas in p.PropertyAspectScores.Where(s => s.WeightedScore >= 65))
                    {
                        aspectInterests.Add(pas.Aspect);
                    }
                }
            }

            // 2. Từ SearchHistory (Filters là JSON do frontend gửi khi user search)
            var recentSearches = await _searchHistoryRepository.GetRecentForUserAsync(userId, limit: 10);
            foreach (var sh in recentSearches)
            {
                if (string.IsNullOrWhiteSpace(sh.Filters)) continue;

                try
                {
                    using var doc = JsonDocument.Parse(sh.Filters);
                    var root = doc.RootElement;

                    // Best-effort parse các key phổ biến (frontend tự quyết định format)
                    if (root.TryGetProperty("areaId", out var areaEl) && areaEl.ValueKind == JsonValueKind.String)
                    {
                        var a = areaEl.GetString();
                        if (!string.IsNullOrEmpty(a)) areaIds.Add(a);
                    }

                    if (root.TryGetProperty("priceMin", out var pmin) && pmin.TryGetDecimal(out var pminVal))
                        priceMin = priceMin.HasValue ? Math.Min(priceMin.Value, pminVal) : pminVal;

                    if (root.TryGetProperty("priceMax", out var pmax) && pmax.TryGetDecimal(out var pmaxVal))
                        priceMax = priceMax.HasValue ? Math.Max(priceMax.Value, pmaxVal) : pmaxVal;

                    if (root.TryGetProperty("amenityIds", out var amArr) && amArr.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var el in amArr.EnumerateArray())
                        {
                            if (el.ValueKind == JsonValueKind.String)
                            {
                                var aid = el.GetString();
                                if (!string.IsNullOrEmpty(aid)) amenityIds.Add(aid);
                            }
                        }
                    }

                    // === MỚI: Parse aspect boosts từ lịch sử search (để recommendation ưu tiên aspect user từng search) ===
                    // Hỗ trợ 2 format phổ biến:
                    // "aspectBoosts": { "Wifi": 1.6, "Noise": 1.6 }
                    // hoặc "boostAspects": ["Wifi", "Noise"] hoặc "preferredAspects"
                    try
                    {
                        JsonElement aspectNode = default;
                        bool hasAspectNode = false;

                        if (root.TryGetProperty("aspectBoosts", out aspectNode) && aspectNode.ValueKind == JsonValueKind.Object)
                            hasAspectNode = true;
                        else if (root.TryGetProperty("boostAspects", out aspectNode) && aspectNode.ValueKind == JsonValueKind.Array)
                            hasAspectNode = true;
                        else if (root.TryGetProperty("preferredAspects", out aspectNode) && aspectNode.ValueKind == JsonValueKind.Array)
                            hasAspectNode = true;

                        if (hasAspectNode)
                        {
                            if (aspectNode.ValueKind == JsonValueKind.Object)
                            {
                                foreach (var prop in aspectNode.EnumerateObject())
                                {
                                    if (Enum.TryParse<ReviewAspect>(prop.Name, ignoreCase: true, out var asp))
                                    {
                                        aspectInterests.Add(asp);
                                    }
                                }
                            }
                            else if (aspectNode.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var el in aspectNode.EnumerateArray())
                                {
                                    if (el.ValueKind == JsonValueKind.String &&
                                        Enum.TryParse<ReviewAspect>(el.GetString(), ignoreCase: true, out var asp))
                                    {
                                        aspectInterests.Add(asp);
                                    }
                                }
                            }
                        }
                    }
                    catch { /* ignore bad aspect data in history */ }

                    // sizeMin/sizeMax cũng có thể parse tương tự nếu cần mở rộng
                }
                catch
                {
                    // ignore malformed JSON – preference vẫn dùng được từ view history
                }
            }

            // Tính mean price từ views (nếu có)
            decimal? priceMean = prices.Count > 0 ? prices.Average() : null;

            // Nếu search có range thì ưu tiên range đó
            decimal? effectiveMin = priceMin ?? (prices.Count > 0 ? prices.Min() : null);
            decimal? effectiveMax = priceMax ?? (prices.Count > 0 ? prices.Max() : null);

            return new UserPreference(areaIds, priceMean, effectiveMin, effectiveMax, amenityIds, aspectInterests);
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
