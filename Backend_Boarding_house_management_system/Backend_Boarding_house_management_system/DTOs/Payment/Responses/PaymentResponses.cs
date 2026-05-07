using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.Payment.Responses
{
    using Backend_Boarding_house_management_system.DTOs.Contract.Responses;
    using Backend_Boarding_house_management_system.DTOs.Invoice.Responses;
    using Backend_Boarding_house_management_system.DTOs.Property.Responses;
    using Backend_Boarding_house_management_system.DTOs.User.Responses;

    public class PaymentResponse
    {
        public string Id { get; set; } = null!;
        public string InvoiceId { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Method { get; set; } = null!;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PaymentListResponse : PagedResponse<PaymentResponse>
    {
    }

    public class PaymentDetailResponse : PaymentResponse
    {
        public InvoiceResponse? Invoice { get; set; }
        public ContractResponse? Contract { get; set; }
        public PropertyResponse? Property { get; set; }
        public UserResponse? Tenant { get; set; }
    }
}
