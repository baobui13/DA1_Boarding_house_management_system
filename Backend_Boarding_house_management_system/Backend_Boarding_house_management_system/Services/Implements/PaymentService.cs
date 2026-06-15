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

        public async Task<PaymentDetailResponse> GetDetailByIdAsync(GetPaymentByIdRequest request)
        {
            var entity = await _paymentRepository.GetByIdWithDetailsAsync(request.Id);
            if (entity == null)
            {
                throw new NotFoundException($"Khong tim thay thanh toan voi Id '{request.Id}'.");
            }
            return _mapper.Map<PaymentDetailResponse>(entity);
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

            // Transaction: payment + invoice status update must be atomic
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await _paymentRepository.AddAsync(entity);

                var totalPaid = await _context.Payments
                    .Where(p => p.InvoiceId == request.InvoiceId)
                    .SumAsync(p => p.Amount);

                invoice.Status = totalPaid >= invoice.Total
                    ? InvoiceStatus.Paid
                    : totalPaid > 0
                        ? InvoiceStatus.Partial
                        : InvoiceStatus.Pending;
                invoice.UpdatedAt = DateTime.UtcNow;

                await _invoiceRepository.UpdateAsync(invoice);

                // Tự động gửi thông báo cho khách thuê khi thanh toán được ghi nhận
                var contract = await _context.Contracts.FindAsync(invoice.ContractId);
                if (contract != null && !string.IsNullOrWhiteSpace(contract.TenantId))
                {
                    var notification = new Notification
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = contract.TenantId,
                        Type = NotificationType.System, // Hoặc Invoice, nhưng Payment hợp lý hơn nếu có
                        Content = $"Thanh toán {entity.Amount:N0}đ cho hóa đơn tháng {invoice.Period:MM/yyyy} đã được ghi nhận thành công.",
                        IsRead = false,
                        Timestamp = DateTime.UtcNow,
                        RelatedId = invoice.Id
                    };
                    _context.Notifications.Add(notification);
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            var savedEntity = await _paymentRepository.GetByIdAsync(entity.Id);
            return _mapper.Map<PaymentResponse>(savedEntity);
        }

        public async Task DeleteAsync(DeletePaymentRequest request)
        {
            var payment = await _paymentRepository.GetByIdAsync(request.Id);
            if (payment == null)
            {
                throw new NotFoundException($"Khong tim thay thanh toan voi Id '{request.Id}'.");
            }

            var invoiceId = payment.InvoiceId;

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await _paymentRepository.DeleteAsync(request.Id);

                var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                if (invoice != null)
                {
                    var totalPaid = await _context.Payments
                        .Where(p => p.InvoiceId == invoiceId)
                        .SumAsync(p => p.Amount);

                    invoice.Status = totalPaid >= invoice.Total
                        ? InvoiceStatus.Paid
                        : totalPaid > 0
                            ? InvoiceStatus.Partial
                            : InvoiceStatus.Pending;
                    invoice.UpdatedAt = DateTime.UtcNow;

                    await _invoiceRepository.UpdateAsync(invoice);
                }

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}
