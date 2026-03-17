using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.Payment.Responses
{
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
}
