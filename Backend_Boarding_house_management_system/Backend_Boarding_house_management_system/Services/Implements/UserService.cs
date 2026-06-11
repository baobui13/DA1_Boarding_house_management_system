using Backend_Boarding_house_management_system.DTOs.User.Requests;
using Backend_Boarding_house_management_system.DTOs.User.Responses;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(AppDbContext context, IUserRepository userRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userRepository = userRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserResponse?> GetUserByIdOrEmailAsync(GetUserByIdOrEmailRequest request)
        {
            User? user = null;

            if (!string.IsNullOrEmpty(request.Id))
                user = await _userRepository.GetByIdAsync(request.Id);
            else if (!string.IsNullOrEmpty(request.Email))
                user = await _userRepository.GetByEmailAsync(request.Email);
            else
                throw new BadRequestException("Phai cung cap Id hoac Email.");

            if (user == null)
                throw new NotFoundException("Khong tim thay nguoi dung.");

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(user.Id, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen xem thong tin nguoi dung nay.");

            return _mapper.Map<UserResponse>(user);
        }

        public async Task<UserListResponse> GetUsersByFilterAsync(
            EntityFilter<User> filter,
            EntitySort<User> sort,
            EntityPage page)
        {
            var isAdmin = IsCurrentUserAdmin();
            var (users, totalCount) = await _userRepository.GetByFilterAsync(filter, sort, page);

            if (!isAdmin)
            {
                // Landlords (and other non-admins) may only list Tenant users.
                // This prevents leaking other user roles while allowing contract/invoice workflows.
                var tenantOnly = users.Where(u => string.Equals(u.Role, "Tenant", StringComparison.OrdinalIgnoreCase)).ToList();
                users = tenantOnly;
                totalCount = tenantOnly.Count;
            }

            return new UserListResponse
            {
                Items = _mapper.Map<List<UserResponse>>(users),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
        }

        public async Task<UserSummaryResponse> GetUserSummaryAsync()
        {
            var now = DateTimeOffset.UtcNow;

            return new UserSummaryResponse
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalActive = await _context.Users.CountAsync(user => user.LockoutEnd == null || user.LockoutEnd <= now),
                TotalLocked = await _context.Users.CountAsync(user => user.LockoutEnd != null && user.LockoutEnd > now),
                TotalLandlords = await _context.Users.CountAsync(user => user.Role == "Landlord")
            };
        }

        public async Task<bool> UpdateUserAsync(UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
                throw new NotFoundException($"Khong tim thay nguoi dung voi Id '{request.Id}'.");

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(user.Id, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen cap nhat thong tin nguoi dung nay.");

            _mapper.Map(request, user);
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> UpdateUserAvatarAsync(UpdateUserAvatarRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
                throw new NotFoundException($"Khong tim thay nguoi dung voi Id '{request.Id}'.");

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(user.Id, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen cap nhat anh dai dien nguoi dung nay.");

            var isSuccess = await _userRepository.UpdateAvatarAsync(request.Id, request.AvatarUrl);
            if (!isSuccess)
                throw new BadRequestException("Cap nhat anh dai dien that bai.");

            return true;
        }

        public async Task<bool> BlockUserAsync(BlockUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
                throw new NotFoundException($"Khong tim thay nguoi dung voi Id '{request.Id}'.");

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(user.Id, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen khoa nguoi dung.");

            var isSuccess = await _userRepository.BlockUserAsync(request.Id, request.IsBlocked);
            if (!isSuccess)
                throw new BadRequestException("Khoa/mo khoa nguoi dung that bai.");

            return true;
        }

        public async Task<bool> DeleteUserAsync(DeleteUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
                throw new NotFoundException($"Khong tim thay nguoi dung voi Id '{request.Id}'.");

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(user.Id, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen xoa nguoi dung.");

            var blockers = await GetUserDeleteBlockersAsync(request.Id);
            if (blockers.Count > 0)
            {
                throw new ConflictException(
                    $"Khong the xoa nguoi dung voi Id '{request.Id}' vi van con du lieu lien quan: {string.Join(", ", blockers)}.");
            }

            try
            {
                await _userRepository.DeleteAsync(request.Id);
            }
            catch (DbUpdateException)
            {
                throw new ConflictException(
                    $"Khong the xoa nguoi dung voi Id '{request.Id}' vi van con du lieu lien quan trong he thong.");
            }

            return true;
        }

        private async Task<List<string>> GetUserDeleteBlockersAsync(string userId)
        {
            var blockers = new List<string>();

            if (await _context.Areas.AnyAsync(x => x.LandlordId == userId))
                blockers.Add("khu vuc");

            if (await _context.Properties.AnyAsync(x => x.LandlordId == userId))
                blockers.Add("bat dong san");

            if (await _context.Contracts.AnyAsync(x => x.TenantId == userId))
                blockers.Add("hop dong thue");

            if (await _context.Appointments.AnyAsync(x => x.UserId == userId))
                blockers.Add("lich hen");

            if (await _context.Messages.AnyAsync(x => x.SenderId == userId || x.ReceiverId == userId))
                blockers.Add("tin nhan");

            if (await _context.Ratings.AnyAsync(x => x.TenantId == userId))
                blockers.Add("danh gia");

            if (await _context.Complaints.AnyAsync(x => x.CreatorId == userId))
                blockers.Add("khieu nai");

            return blockers;
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
