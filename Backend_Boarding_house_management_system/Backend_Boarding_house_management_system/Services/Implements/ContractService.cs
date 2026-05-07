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

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class ContractService : IContractService
    {
        private readonly AppDbContext _context;
        private readonly IContractRepository _contractRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public ContractService(
            AppDbContext context,
            IContractRepository contractRepository,
            IPropertyRepository propertyRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _context = context;
            _contractRepository = contractRepository;
            _propertyRepository = propertyRepository;
            _userRepository = userRepository;
            _mapper = mapper;
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

            var tenant = await _userRepository.GetByIdAsync(request.TenantId);
            if (tenant == null)
                throw new NotFoundException($"Khong tim thay tenant voi Id '{request.TenantId}'.");

            if (!string.Equals(tenant.Role, "Tenant", StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("User duoc chon khong phai tenant.");

            var entity = _mapper.Map<Contract>(request);
            entity.Id = Guid.NewGuid().ToString();
            entity.CreatedAt = DateTime.UtcNow;
            entity.Status = "Active";

            await _contractRepository.AddAsync(entity);

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

            _mapper.Map(request, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            await _contractRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(DeleteContractRequest request)
        {
            var existing = await _contractRepository.GetByIdAsync(request.Id);
            if (existing == null)
            {
                throw new NotFoundException($"Khong tim thay hop dong voi Id '{request.Id}'.");
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

            if (string.Equals(contract.Status, "Active", StringComparison.OrdinalIgnoreCase))
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
    }
}
