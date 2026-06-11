using Backend_Boarding_house_management_system.DTOs.Area.Requests;
using Backend_Boarding_house_management_system.DTOs.Area.Responses;
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
    public class AreaService : IAreaService
    {
        private readonly AppDbContext _context;
        private readonly IAreaRepository _areaRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AreaService(AppDbContext context, IAreaRepository areaRepository, IUserRepository userRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _areaRepository = areaRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AreaResponse> GetAreaByIdAsync(GetAreaByIdRequest request)
        {
            if (string.IsNullOrEmpty(request.Id))
                throw new BadRequestException("Phai cung cap Id.");

            var area = await _areaRepository.GetByIdAsync(request.Id);
            if (area == null)
                throw new NotFoundException("Khong tim thay khu vuc.");

            return _mapper.Map<AreaResponse>(area);
        }

        public async Task<AreaDetailResponse> GetAreaDetailByIdAsync(GetAreaByIdRequest request)
        {
            if (string.IsNullOrEmpty(request.Id))
                throw new BadRequestException("Phai cung cap Id.");

            var area = await _areaRepository.GetByIdWithDetailsAsync(request.Id);
            if (area == null)
                throw new NotFoundException("Khong tim thay khu vuc.");

            return _mapper.Map<AreaDetailResponse>(area);
        }

        public async Task<AreaListResponse> GetAreasByFilterAsync(
            EntityFilter<Area> filter,
            EntitySort<Area> sort,
            EntityPage page)
        {
            var (areas, totalCount) = await _areaRepository.GetByFilterAsync(filter, sort, page);
            return new AreaListResponse
            {
                Items = _mapper.Map<List<AreaResponse>>(areas),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
        }

        public async Task<AreaResponse> CreateAreaAsync(CreateAreaRequest request)
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(request.LandlordId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban chi duoc tao khu vuc cho chinh minh.");

            var landlord = await _userRepository.GetByIdAsync(request.LandlordId);
            if (landlord == null)
                throw new NotFoundException($"Khong tim thay landlord voi Id '{request.LandlordId}'.");

            if (!string.Equals(landlord.Role, "Landlord", StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("User duoc chon khong phai landlord.");

            var area = _mapper.Map<Area>(request);
            area.Id = Guid.NewGuid().ToString();
            area.CreatedAt = DateTime.UtcNow;

            await _areaRepository.AddAsync(area);
            return _mapper.Map<AreaResponse>(area);
        }

        public async Task<bool> UpdateAreaAsync(UpdateAreaRequest request)
        {
            var area = await _areaRepository.GetByIdAsync(request.Id);
            if (area == null)
                throw new NotFoundException($"Khong tim thay khu vuc voi Id '{request.Id}'.");

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(area.LandlordId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen cap nhat khu vuc nay.");

            _mapper.Map(request, area);
            await _areaRepository.UpdateAsync(area);
            return true;
        }

        public async Task<bool> UpdateAreaDescriptionAsync(UpdateAreaDescriptionRequest request)
        {
            var area = await _areaRepository.GetByIdAsync(request.Id);
            if (area == null)
                throw new NotFoundException($"Khong tim thay khu vuc voi Id '{request.Id}'.");

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(area.LandlordId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen cap nhat khu vuc nay.");

            area.Description = request.Description;
            await _areaRepository.UpdateAsync(area);
            return true;
        }

        public async Task<bool> DeleteAreaAsync(DeleteAreaRequest request)
        {
            var area = await _areaRepository.GetByIdAsync(request.Id);
            if (area == null)
                throw new NotFoundException($"Khong tim thay khu vuc voi Id '{request.Id}'.");

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(area.LandlordId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen xoa khu vuc nay.");

            if (await _context.Properties.AnyAsync(p => p.AreaId == request.Id))
                throw new ConflictException("Khong the xoa khu vuc vi con bat dong san.");

            await _areaRepository.DeleteAsync(request.Id);
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
