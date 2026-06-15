using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.DTOs.Contract.Requests;
using Backend_Boarding_house_management_system.DTOs.Contract.Responses;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class ContractService : IContractService
    {
        private readonly AppDbContext _context;
        private readonly IContractRepository _contractRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContractService(
            AppDbContext context,
            IContractRepository contractRepository,
            IPropertyRepository propertyRepository,
            IUserRepository userRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _contractRepository = contractRepository;
            _propertyRepository = propertyRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ContractResponse> GetByIdAsync(GetContractByIdRequest request)
        {
            var entity = await _contractRepository.GetByIdAsync(request.Id);
            if (entity == null)
            {
                throw new NotFoundException($"Khong tim thay hop dong voi Id '{request.Id}'.");
            }
            return _mapper.Map<ContractResponse>(entity);
        }

        public async Task<ContractListResponse> GetByFilterAsync(
            EntityFilter<Contract> filter,
            EntitySort<Contract> sort,
            EntityPage page)
        {
            var (items, totalCount) = await _contractRepository.GetByFilterAsync(filter, sort, page);
            var response = new ContractListResponse
            {
                Items = _mapper.Map<List<ContractResponse>>(items),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
            return response;
        }

        public async Task<ContractResponse> CreateAsync(CreateContractRequest request)
        {
            var property = await _propertyRepository.GetByIdAsync(request.PropertyId);
            if (property == null)
                throw new NotFoundException($"Khong tim thay phong voi Id '{request.PropertyId}'.");

            // Ownership check: Landlord caller must own the property (or Admin)
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin && !string.Equals(property.LandlordId, currentUserId, StringComparison.Ordinal))
                throw new ForbiddenException("Ban khong phai chu so huu cua phong nay.");

            var tenant = await _userRepository.GetByIdAsync(request.TenantId);
            if (tenant == null)
                throw new NotFoundException($"Khong tim thay tenant voi Id '{request.TenantId}'.");

            if (!string.Equals(tenant.Role, "Tenant", StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("User duoc chon khong phai tenant.");

            var hasActiveContract = await _context.Contracts.AnyAsync(contract =>
                contract.PropertyId == request.PropertyId &&
                IsOccupyingContractStatus(contract.Status.ToString()));
            if (hasActiveContract)
                throw new ConflictException("Phong nay da co hop dong dang hieu luc.");

            var entity = _mapper.Map<Contract>(request);
            entity.Id = Guid.NewGuid().ToString();
            entity.CreatedAt = DateTime.UtcNow;
            entity.Status = ContractStatus.Active;

            // Transaction: ensure contract + property availability are atomic
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await _contractRepository.AddAsync(entity);
                await SyncPropertyAvailabilityAsync(request.PropertyId);
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            // Tự động tạo thông báo gửi cho khách thuê khi kích hoạt hợp đồng
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = request.TenantId,
                Type = NotificationType.Contract,
                Content = $"Hợp đồng thuê {property.PropertyName} đang có hiệu lực đến ngày {entity.EndDate:dd/MM/yyyy}.",
                IsRead = false,
                Timestamp = DateTime.UtcNow,
                RelatedId = entity.Id
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var savedEntity = await _contractRepository.GetByIdAsync(entity.Id);
            return _mapper.Map<ContractResponse>(savedEntity);
        }

        public async Task UpdateAsync(UpdateContractRequest request)
        {
            var existing = await _contractRepository.GetByIdAsync(request.Id);
            if (existing == null)
            {
                throw new NotFoundException($"Khong tim thay hop dong voi Id '{request.Id}'.");
            }

            // Ownership check
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin)
            {
                var prop = await _propertyRepository.GetByIdAsync(existing.PropertyId);
                if (prop == null || !string.Equals(prop.LandlordId, currentUserId, StringComparison.Ordinal))
                    throw new ForbiddenException("Ban khong co quyen cap nhat hop dong nay.");
            }

            var nextStatus = string.IsNullOrWhiteSpace(request.Status) ? existing.Status.ToString() : request.Status;
            ValidateStatusTransition(existing.Status.ToString(), nextStatus);

            var isActivating = IsOccupyingContractStatus(nextStatus);
            if (isActivating)
            {
                var hasAnotherActiveContract = await _context.Contracts.AnyAsync(contract =>
                    contract.PropertyId == existing.PropertyId &&
                    contract.Id != existing.Id &&
                    IsOccupyingContractStatus(contract.Status.ToString()));
                if (hasAnotherActiveContract)
                    throw new ConflictException("Phong nay da co hop dong dang hieu luc khac.");
            }

            _mapper.Map(request, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            // Transaction for contract update + property sync
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await _contractRepository.UpdateAsync(existing);
                await SyncPropertyAvailabilityAsync(existing.PropertyId);
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteAsync(DeleteContractRequest request)
        {
            var existing = await _contractRepository.GetByIdAsync(request.Id);
            if (existing == null)
            {
                throw new NotFoundException($"Khong tim thay hop dong voi Id '{request.Id}'.");
            }

            // Ownership check
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin)
            {
                var prop = await _propertyRepository.GetByIdAsync(existing.PropertyId);
                if (prop == null || !string.Equals(prop.LandlordId, currentUserId, StringComparison.Ordinal))
                    throw new ForbiddenException("Ban khong co quyen xoa hop dong nay.");
            }

            var blockers = await GetContractDeleteBlockersAsync(existing);
            if (blockers.Count > 0)
            {
                throw new ConflictException(
                    $"Khong the xoa hop dong voi Id '{request.Id}' vi van con du lieu lien quan: {string.Join(", ", blockers)}.");
            }

            try
            {
                await _contractRepository.DeleteAsync(request.Id);
            }
            catch (DbUpdateException)
            {
                throw new ConflictException(
                    $"Khong the xoa hop dong voi Id '{request.Id}' vi van con du lieu lien quan trong he thong.");
            }
        }

        private async Task<List<string>> GetContractDeleteBlockersAsync(Contract contract)
        {
            var blockers = new List<string>();

            if (string.Equals(contract.Status.ToString(), ContractStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase))
                blockers.Add("hop dong dang hoat dong");

            if (await _context.Invoices.AnyAsync(x => x.ContractId == contract.Id))
                blockers.Add("hoa don");

            var hasPayments = await _context.Payments.AnyAsync(x => x.Invoice.ContractId == contract.Id);
            if (hasPayments)
                blockers.Add("thanh toan");

            if (await _context.Messages.AnyAsync(x => x.ContractId == contract.Id))
                blockers.Add("tin nhan");

            return blockers;
        }

        private async Task SyncPropertyAvailabilityAsync(string propertyId)
        {
            var property = await _propertyRepository.GetByIdAsync(propertyId);
            if (property == null)
            {
                return;
            }

            var hasOccupyingContract = await _context.Contracts.AnyAsync(contract =>
                contract.PropertyId == propertyId &&
                IsOccupyingContractStatus(contract.Status.ToString()));

            if (hasOccupyingContract)
            {
                property.AvailabilityStatus = AvailabilityStatusEnum.Rented;
            }
            else if (property.AvailabilityStatus == AvailabilityStatusEnum.Rented)
            {
                property.AvailabilityStatus = AvailabilityStatusEnum.Available;
            }

            property.UpdatedAt = DateTime.UtcNow;
            await _propertyRepository.UpdateAsync(property);
        }

        private static bool IsOccupyingContractStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }
            var s = status.Trim();
            // Support both enum names and legacy strings for backward compat
            return s.Equals(ContractStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase) ||
                   s.Equals(ContractStatus.NearExpiry.ToString(), StringComparison.OrdinalIgnoreCase) ||
                   s.Equals("Signed", StringComparison.OrdinalIgnoreCase) ||
                   s.Equals("Approved", StringComparison.OrdinalIgnoreCase);
        }

        private static void ValidateStatusTransition(string current, string next)
        {
            if (string.IsNullOrWhiteSpace(next) || string.Equals(current, next, StringComparison.OrdinalIgnoreCase))
                return;

            // Basic allowed transitions (reasonable flow)
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ContractStatus.Draft.ToString(),
                ContractStatus.Active.ToString(),
                ContractStatus.NearExpiry.ToString(),
                ContractStatus.Expired.ToString(),
                ContractStatus.Terminated.ToString(),
                ContractStatus.Cancelled.ToString(),
                "Signed", "Approved" // legacy support
            };

            if (!allowed.Contains(next))
                throw new BadRequestException($"Trang thai hop dong '{next}' khong hop le.");

            // Simple transition rules
            if (string.Equals(current, ContractStatus.Expired.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(current, ContractStatus.Terminated.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(current, ContractStatus.Cancelled.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Khong the thay doi trang thai tu hop dong da ket thuc.");
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
