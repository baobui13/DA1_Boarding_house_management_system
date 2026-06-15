using AutoMapper;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.DTOs.Complaint.Requests;
using Backend_Boarding_house_management_system.DTOs.Complaint.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Plainquire.Filter;
using Plainquire.Page;
using Plainquire.Sort;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class ComplaintService : IComplaintService
    {
        private readonly IComplaintRepository _complaintRepository;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ComplaintService(
            IComplaintRepository complaintRepository,
            IUserRepository userRepository,
            AppDbContext context,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _complaintRepository = complaintRepository;
            _userRepository = userRepository;
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ComplaintResponse> GetComplaintByIdAsync(GetComplaintByIdRequest request)
        {
            var complaint = await _complaintRepository.GetByIdAsync(request.Id);
            if (complaint == null)
                throw new NotFoundException($"Khong tim thay khieu nai voi Id '{request.Id}'.");

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(complaint.CreatorId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen xem khieu nai nay.");

            return _mapper.Map<ComplaintResponse>(complaint);
        }

        public async Task<ComplaintDetailResponse> GetComplaintDetailByIdAsync(GetComplaintByIdRequest request)
        {
            var complaint = await _complaintRepository.GetByIdWithDetailsAsync(request.Id);
            if (complaint == null)
                throw new NotFoundException($"Khong tim thay khieu nai voi Id '{request.Id}'.");

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(complaint.CreatorId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen xem khieu nai nay.");

            return _mapper.Map<ComplaintDetailResponse>(complaint);
        }

        public async Task<ComplaintListResponse> GetComplaintsByFilterAsync(
            EntityFilter<Complaint> filter,
            EntitySort<Complaint> sort,
            EntityPage page, string? landlordId = null)
        {
            var (complaints, totalCount) = await _complaintRepository.GetByFilterWithDetailsAsync(filter, sort, page, landlordId);
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            var filtered = complaints.Where(c => isAdmin || string.Equals(c.CreatorId, currentUserId, StringComparison.Ordinal)).ToList();

            return new ComplaintListResponse
            {
                Items = _mapper.Map<List<ComplaintResponse>>(filtered),
                TotalCount = filtered.Count,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
        }

        public async Task<ComplaintResponse> CreateComplaintAsync(CreateComplaintRequest request)
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(request.CreatorId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban chi duoc tao khieu nai voi tai khoan cua minh.");

            if (await _userRepository.GetByIdAsync(request.CreatorId) == null)
                throw new NotFoundException($"Khong tim thay user voi Id '{request.CreatorId}'.");

            await ValidateRelatedEntityAsync(request.RelatedType, request.RelatedId);

            // Basic relation check for property
            if (string.Equals(request.RelatedType, "property", StringComparison.OrdinalIgnoreCase))
            {
                var prop = await _context.Properties.FindAsync(request.RelatedId);
                if (prop != null && !isAdmin && !string.Equals(prop.LandlordId, currentUserId, StringComparison.Ordinal))
                {
                    var hasContract = await _context.Contracts.AnyAsync(c => c.PropertyId == request.RelatedId && c.TenantId == currentUserId);
                    if (!hasContract)
                        throw new ForbiddenException("Ban khong co quyen khieu nai ve bat dong san nay.");
                }
            }

            var complaint = _mapper.Map<Complaint>(request);
            complaint.Id = Guid.NewGuid().ToString();
            complaint.CreatedAt = DateTime.UtcNow;
            complaint.Status = ComplaintStatus.Pending;

            await _complaintRepository.AddAsync(complaint);

            // Auto notify (simple: for admin or related landlord)
            if (!string.IsNullOrWhiteSpace(request.RelatedId) && string.Equals(request.RelatedType, "property", StringComparison.OrdinalIgnoreCase))
            {
                var prop = await _context.Properties.FindAsync(request.RelatedId);
                if (prop != null)
                {
                    var notif = new Notification
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = prop.LandlordId,
                        Type = NotificationType.System,
                        Content = "Co khieu nai moi ve bat dong san cua ban.",
                        IsRead = false,
                        Timestamp = DateTime.UtcNow,
                        RelatedId = complaint.Id
                    };
                    _context.Notifications.Add(notif);
                    await _context.SaveChangesAsync();
                }
            }

            return _mapper.Map<ComplaintResponse>(complaint);
        }

        public async Task<bool> UpdateComplaintAsync(UpdateComplaintRequest request)
        {
            var complaint = await _complaintRepository.GetByIdAsync(request.Id);
            if (complaint == null)
                throw new NotFoundException($"Khong tim thay khieu nai voi Id '{request.Id}'.");

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(complaint.CreatorId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen cap nhat khieu nai nay.");

            var relatedType = request.RelatedType ?? complaint.RelatedType.ToString();
            var relatedId = request.RelatedId ?? complaint.RelatedId;
            await ValidateRelatedEntityAsync(relatedType, relatedId);

            _mapper.Map(request, complaint);

            if (string.Equals(complaint.Status.ToString(), "Resolved", StringComparison.OrdinalIgnoreCase) && complaint.ResolvedAt == null)
            {
                complaint.ResolvedAt = DateTime.UtcNow;
            }

            await _complaintRepository.UpdateAsync(complaint);
            return true;
        }

        public async Task<bool> DeleteComplaintAsync(DeleteComplaintRequest request)
        {
            var complaint = await _complaintRepository.GetByIdAsync(request.Id);
            if (complaint == null)
                throw new NotFoundException($"Khong tim thay khieu nai voi Id '{request.Id}'.");

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(complaint.CreatorId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong co quyen xoa khieu nai nay.");

            await _complaintRepository.DeleteAsync(request.Id);
            return true;
        }

        private async Task ValidateRelatedEntityAsync(string relatedType, string relatedId)
        {
            if (string.IsNullOrWhiteSpace(relatedType))
                throw new BadRequestException("RelatedType khong duoc de trong.");

            if (string.IsNullOrWhiteSpace(relatedId))
                throw new BadRequestException("RelatedId khong duoc de trong.");

            var exists = relatedType.Trim().ToLowerInvariant() switch
            {
                "invoice" => await _context.Invoices.AnyAsync(x => x.Id == relatedId),
                "contract" => await _context.Contracts.AnyAsync(x => x.Id == relatedId),
                "property" => await _context.Properties.AnyAsync(x => x.Id == relatedId),
                _ => throw new BadRequestException("RelatedType phai la Invoice, Contract hoac Property.")
            };

            if (!exists)
                throw new NotFoundException($"Khong tim thay doi tuong lien quan voi RelatedType '{relatedType}' va RelatedId '{relatedId}'.");
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
