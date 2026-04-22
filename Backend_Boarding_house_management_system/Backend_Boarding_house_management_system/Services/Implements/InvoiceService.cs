using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.DTOs.Invoice.Requests;
using Backend_Boarding_house_management_system.DTOs.Invoice.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using AutoMapper;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IMapper _mapper;

        public InvoiceService(IInvoiceRepository invoiceRepository, IContractRepository contractRepository, IMapper mapper)
        {
            _invoiceRepository = invoiceRepository;
            _contractRepository = contractRepository;
            _mapper = mapper;
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
            if (await _contractRepository.GetByIdAsync(request.ContractId) == null)
                throw new NotFoundException($"Khong tim thay hop dong voi Id '{request.ContractId}'.");

            var entity = _mapper.Map<Invoice>(request);
            entity.Id = Guid.NewGuid().ToString();
            entity.CreatedAt = DateTime.UtcNow;

            await _invoiceRepository.AddAsync(entity);

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

            _mapper.Map(request, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            await _invoiceRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(DeleteInvoiceRequest request)
        {
            var exists = await _invoiceRepository.ExistsAsync(request.Id);
            if (!exists)
            {
                throw new NotFoundException($"Khong tim thay hoa don voi Id '{request.Id}'.");
            }
            await _invoiceRepository.DeleteAsync(request.Id);
        }
    }
}
