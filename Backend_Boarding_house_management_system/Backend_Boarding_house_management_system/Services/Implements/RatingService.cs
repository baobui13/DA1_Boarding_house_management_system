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

        public RatingService(
            AppDbContext context,
            IRatingRepository ratingRepository,
            IUserRepository userRepository,
            IPropertyRepository propertyRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _ratingRepository = ratingRepository;
            _userRepository = userRepository;
            _propertyRepository = propertyRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
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
            EntityPage page, string? landlordId = null)
        {
            var (ratings, totalCount) = await _ratingRepository.GetByFilterWithDetailsAsync(filter, sort, page, landlordId);
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

            var property = await _propertyRepository.GetByIdAsync(request.PropertyId);
            if (property == null)
                throw new NotFoundException($"Khong tim thay bat dong san voi Id '{request.PropertyId}'.");

            if (!await _context.Contracts.AnyAsync(c => c.TenantId == request.TenantId && c.PropertyId == request.PropertyId))
                throw new BadRequestException("Ban chua tung co hop dong voi bat dong san nay.");

            if (await _ratingRepository.ExistsByTenantAndPropertyAsync(request.TenantId, request.PropertyId))
                throw new ConflictException("Tenant nay da danh gia bat dong san nay roi.");

            var rating = _mapper.Map<Rating>(request);
            rating.Id = Guid.NewGuid().ToString();
            rating.CreatedAt = DateTime.UtcNow;

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await _ratingRepository.AddAsync(rating);

                // Update landlord reputation (+1 on new rating)
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
                        Type = NotificationType.Rating,
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

            // Tự động thông báo cho Landlord khi có đánh giá mới
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = property.LandlordId,
                Type = NotificationType.Rating,
                Content = $"Khách thuê {tenant.FullName} đã gửi đánh giá mới ({rating.Stars} sao) cho phòng \"{property.PropertyName}\".",
                IsRead = false,
                Timestamp = DateTime.UtcNow,
                RelatedId = rating.Id
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

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

            return true;
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
