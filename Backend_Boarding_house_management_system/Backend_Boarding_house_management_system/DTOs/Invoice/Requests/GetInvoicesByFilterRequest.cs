using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.Invoice.Requests
{
    public class GetInvoicesByFilterRequest : PagedRequest
    {
        public string? ContractId { get; set; }
        public string? Status { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
    }
}
