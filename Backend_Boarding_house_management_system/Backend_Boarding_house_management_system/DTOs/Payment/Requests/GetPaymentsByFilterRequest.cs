using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.Payment.Requests
{
    public class GetPaymentsByFilterRequest : PagedRequest
    {
        public string? InvoiceId { get; set; }
        public string? Method { get; set; }
        public DateTime? PaymentDateFrom { get; set; }
        public DateTime? PaymentDateTo { get; set; }
    }
}
