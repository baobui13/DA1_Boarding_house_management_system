using Backend_Boarding_house_management_system.DTOs.Payment.Requests;
using Backend_Boarding_house_management_system.DTOs.Payment.Responses;
using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponse> GetByIdAsync(GetPaymentByIdRequest request);
        Task<PaymentDetailResponse> GetDetailByIdAsync(GetPaymentByIdRequest request);
        Task<PaymentListResponse> GetByFilterAsync(
            EntityFilter<Payment> filter,
            EntitySort<Payment> sort,
            EntityPage page);
        Task<PaymentResponse> CreateAsync(CreatePaymentRequest request);
        Task DeleteAsync(DeletePaymentRequest request);
    }
}
