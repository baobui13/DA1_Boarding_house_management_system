using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.DTOs.Invoice.Requests;
using Backend_Boarding_house_management_system.DTOs.Invoice.Responses;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using AutoMapper;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class InvoiceService : IInvoiceService
    {
        private readonly AppDbContext _context;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InvoiceService(AppDbContext context, IInvoiceRepository invoiceRepository, IContractRepository contractRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _invoiceRepository = invoiceRepository;
            _contractRepository = contractRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<InvoiceResponse> GetByIdAsync(GetInvoiceByIdRequest request)
        {
            var entity = await _invoiceRepository.GetByIdAsync(request.Id);
            if (entity == null)
            {
                throw new NotFoundException($"Khong tim thay hoa don voi Id '{request.Id}'.");
            }
            return _mapper.Map<InvoiceResponse>(entity);
        }

        public async Task<InvoiceDetailResponse> GetDetailByIdAsync(GetInvoiceByIdRequest request)
        {
            var entity = await _invoiceRepository.GetByIdWithDetailsAsync(request.Id);
            if (entity == null)
            {
                throw new NotFoundException($"Khong tim thay hoa don voi Id '{request.Id}'.");
            }
            return _mapper.Map<InvoiceDetailResponse>(entity);
        }

        public async Task<InvoiceListResponse> GetByFilterAsync(
            EntityFilter<Invoice> filter,
            EntitySort<Invoice> sort,
            EntityPage page)
        {
            var (items, totalCount) = await _invoiceRepository.GetByFilterAsync(filter, sort, page);
            var response = new InvoiceListResponse
            {
                Items = _mapper.Map<List<InvoiceResponse>>(items),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
            return response;
        }

        public async Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request)
        {
            var contract = await _contractRepository.GetByIdAsync(request.ContractId);
            if (contract == null)
                throw new NotFoundException($"Khong tim thay hop dong voi Id '{request.ContractId}'.");

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin)
            {
                // load property for landlord check, or check tenant
                var prop = await _context.Properties.FindAsync(contract.PropertyId);
                if (prop == null || (!string.Equals(prop.LandlordId, currentUserId, StringComparison.Ordinal) && !string.Equals(contract.TenantId, currentUserId, StringComparison.Ordinal)))
                    throw new ForbiddenException("Ban khong co quyen tao hoa don cho hop dong nay.");
            }

            var entity = _mapper.Map<Invoice>(request);
            entity.Id = Guid.NewGuid().ToString();
            entity.CreatedAt = DateTime.UtcNow;

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await _invoiceRepository.AddAsync(entity);
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            var savedEntity = await _invoiceRepository.GetByIdAsync(entity.Id);
            return _mapper.Map<InvoiceResponse>(savedEntity);
        }

        public async Task UpdateAsync(UpdateInvoiceRequest request)
        {
            var existing = await _invoiceRepository.GetByIdAsync(request.Id);
            if (existing == null)
            {
                throw new NotFoundException($"Khong tim thay hoa don voi Id '{request.Id}'.");
            }

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin)
            {
                var contract = await _contractRepository.GetByIdAsync(existing.ContractId);
                if (contract == null)
                    throw new NotFoundException("Khong tim thay hop dong.");
                var prop = await _context.Properties.FindAsync(contract.PropertyId);
                if (prop == null || (!string.Equals(prop.LandlordId, currentUserId, StringComparison.Ordinal) && !string.Equals(contract.TenantId, currentUserId, StringComparison.Ordinal)))
                    throw new ForbiddenException("Ban khong co quyen cap nhat hoa don nay.");
            }

            _mapper.Map(request, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            await _invoiceRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(DeleteInvoiceRequest request)
        {
            var existing = await _invoiceRepository.GetByIdAsync(request.Id);
            if (existing == null)
            {
                throw new NotFoundException($"Khong tim thay hoa don voi Id '{request.Id}'.");
            }

            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            if (!isAdmin)
            {
                var contract = await _contractRepository.GetByIdAsync(existing.ContractId);
                if (contract == null)
                    throw new NotFoundException("Khong tim thay hop dong.");
                var prop = await _context.Properties.FindAsync(contract.PropertyId);
                if (prop == null || (!string.Equals(prop.LandlordId, currentUserId, StringComparison.Ordinal) && !string.Equals(contract.TenantId, currentUserId, StringComparison.Ordinal)))
                    throw new ForbiddenException("Ban khong co quyen xoa hoa don nay.");
            }

            await _invoiceRepository.DeleteAsync(request.Id);
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
