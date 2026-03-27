using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.DTOs.Payment.Requests;
using Backend_Boarding_house_management_system.DTOs.Payment.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using AutoMapper;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;

        public PaymentService(IPaymentRepository paymentRepository, IMapper mapper)
        {
            _paymentRepository = paymentRepository;
            _mapper = mapper;
        }

        public async Task<PaymentResponse> GetByIdAsync(GetPaymentByIdRequest request)
        {
            var entity = await _paymentRepository.GetByIdAsync(request.Id);
            if (entity == null)
            {
                throw new NotFoundException($"Không tìm thấy thanh toán với Id '{request.Id}'.");
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
            var entity = _mapper.Map<Payment>(request);
            entity.Id = Guid.NewGuid().ToString();
            entity.PaymentDate = DateTime.UtcNow;
            entity.CreatedAt = DateTime.UtcNow;

            await _paymentRepository.AddAsync(entity);

            var savedEntity = await _paymentRepository.GetByIdAsync(entity.Id);
            return _mapper.Map<PaymentResponse>(savedEntity);
        }

        public async Task DeleteAsync(DeletePaymentRequest request)
        {
            var exists = await _paymentRepository.ExistsAsync(request.Id);
            if (!exists)
            {
                throw new NotFoundException($"Không tìm thấy thanh toán với Id '{request.Id}'.");
            }
            await _paymentRepository.DeleteAsync(request.Id);
        }
    }
}
