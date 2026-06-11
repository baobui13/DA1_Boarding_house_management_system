using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.DTOs.Notification.Requests;
using Backend_Boarding_house_management_system.DTOs.Notification.Responses;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationService(
            INotificationRepository notificationRepository,
            IUserRepository userRepository,
            AppDbContext context,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<NotificationResponse> GetByIdAsync(GetNotificationByIdRequest request)
        {
            var entity = await _notificationRepository.GetByIdAsync(request.Id);
            if (entity == null)
            {
                throw new NotFoundException($"Khong tim thay thong bao voi Id '{request.Id}'.");
            }

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(entity.UserId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen xem thong bao nay.");

            return _mapper.Map<NotificationResponse>(entity);
        }

        public async Task<NotificationListResponse> GetByFilterAsync(
            EntityFilter<Notification> filter,
            EntitySort<Notification> sort,
            EntityPage page)
        {
            var (items, totalCount) = await _notificationRepository.GetByFilterAsync(filter, sort, page);
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            var filtered = items.Where(n => isAdmin || string.Equals(n.UserId, currentUserId, StringComparison.Ordinal)).ToList();

            var response = new NotificationListResponse
            {
                Items = _mapper.Map<List<NotificationResponse>>(filtered),
                TotalCount = filtered.Count,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
            return response;
        }

        public async Task<NotificationResponse> CreateAsync(CreateNotificationRequest request)
        {
            if (await _userRepository.GetByIdAsync(request.UserId) == null)
                throw new NotFoundException($"Khong tim thay nguoi dung voi Id '{request.UserId}'.");

            var notificationType = request.Type?.Trim();
            if (string.IsNullOrWhiteSpace(notificationType))
                throw new BadRequestException("Loai thong bao khong hop le.");

            if (!string.Equals(notificationType, "System", StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(request.RelatedId))
            {
                throw new BadRequestException("RelatedId la bat buoc voi loai thong bao nay.");
            }

            if (!string.IsNullOrWhiteSpace(request.RelatedId))
            {
                var exists = await RelatedEntityExistsAsync(notificationType, request.RelatedId);
                if (!exists)
                    throw new NotFoundException($"Khong tim thay du lieu lien quan voi Id '{request.RelatedId}'.");
            }

            var entity = _mapper.Map<Notification>(request);
            entity.Id = Guid.NewGuid().ToString();
            entity.Timestamp = DateTime.UtcNow;
            entity.IsRead = false;

            await _notificationRepository.AddAsync(entity);

            var savedEntity = await _notificationRepository.GetByIdAsync(entity.Id);
            return _mapper.Map<NotificationResponse>(savedEntity);
        }

        public async Task UpdateAsync(UpdateNotificationRequest request)
        {
            var existing = await _notificationRepository.GetByIdAsync(request.Id);
            if (existing == null)
            {
                throw new NotFoundException($"Khong tim thay thong bao voi Id '{request.Id}'.");
            }

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(existing.UserId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen cap nhat thong bao nay.");

            existing.IsRead = request.IsRead;

            await _notificationRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(DeleteNotificationRequest request)
        {
            var existing = await _notificationRepository.GetByIdAsync(request.Id);
            if (existing == null)
            {
                throw new NotFoundException($"Khong tim thay thong bao voi Id '{request.Id}'.");
            }

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(existing.UserId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen xoa thong bao nay.");

            await _notificationRepository.DeleteAsync(request.Id);
        }

        private async Task<bool> RelatedEntityExistsAsync(string notificationType, string relatedId)
        {
            return notificationType.ToLowerInvariant() switch
            {
                "invoice" => await _context.Invoices.AnyAsync(x => x.Id == relatedId),
                "appointment" => await _context.Appointments.AnyAsync(x => x.Id == relatedId),
                "contract" => await _context.Contracts.AnyAsync(x => x.Id == relatedId),
                "message" => await _context.Messages.AnyAsync(x => x.Id == relatedId),
                "rating" => await _context.Ratings.AnyAsync(x => x.Id == relatedId),
                "system" => true,
                _ => throw new BadRequestException("Loai thong bao khong ho tro.")
            };
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
