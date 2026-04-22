using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.DTOs.Payment.Requests;
using Backend_Boarding_house_management_system.DTOs.Payment.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using Backend_Boarding_house_management_system.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IInvoiceRepository invoiceRepository,
            AppDbContext context,
            IMapper mapper)
        {
            _paymentRepository = paymentRepository;
            _invoiceRepository = invoiceRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaymentResponse> GetByIdAsync(GetPaymentByIdRequest request)
        {
            var entity = await _paymentRepository.GetByIdAsync(request.Id);
            if (entity == null)
            {
                throw new NotFoundException($"Khong tim thay thanh toan voi Id '{request.Id}'.");
            }
            return _mapper.Map<PaymentResponse>(entity);
        }

        public async Task<PaymentListResponse> GetByFilterAsync(
            EntityFilter<Payment> filter,
            EntitySort<Payment> sort,
            EntityPage page)
        {
            var (items, totalCount) = await _paymentRepository.GetByFilterAsync(filter, sort, page);
            var response = new PaymentListResponse
            {
                Items = _mapper.Map<List<PaymentResponse>>(items),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
            return response;
        }

        public async Task<PaymentResponse> CreateAsync(CreatePaymentRequest request)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId);
            if (invoice == null)
                throw new NotFoundException($"Khong tim thay hoa don voi Id '{request.InvoiceId}'.");

            if (request.Amount <= 0)
                throw new BadRequestException("So tien thanh toan phai lon hon 0.");

            var entity = _mapper.Map<Payment>(request);
            entity.Id = Guid.NewGuid().ToString();
            entity.PaymentDate = DateTime.UtcNow;
            entity.CreatedAt = DateTime.UtcNow;

            await _paymentRepository.AddAsync(entity);

            var totalPaid = await _context.Payments
                .Where(p => p.InvoiceId == request.InvoiceId)
                .SumAsync(p => p.Amount);

            invoice.Status = totalPaid >= invoice.Total
                ? "Paid"
                : totalPaid > 0
                    ? "Partial"
                    : "Pending";
            invoice.UpdatedAt = DateTime.UtcNow;

            await _invoiceRepository.UpdateAsync(invoice);

            var savedEntity = await _paymentRepository.GetByIdAsync(entity.Id);
            return _mapper.Map<PaymentResponse>(savedEntity);
        }

        public async Task DeleteAsync(DeletePaymentRequest request)
        {
            var exists = await _paymentRepository.ExistsAsync(request.Id);
            if (!exists)
            {
                throw new NotFoundException($"Khong tim thay thanh toan voi Id '{request.Id}'.");
            }
            await _paymentRepository.DeleteAsync(request.Id);
        }
    }
}
