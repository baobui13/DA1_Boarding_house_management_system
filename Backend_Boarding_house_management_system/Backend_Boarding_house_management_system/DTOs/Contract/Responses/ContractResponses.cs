using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.Contract.Responses
{
    public class ContractResponse
    {
        public string Id { get; set; } = null!;
        public string PropertyId { get; set; } = null!;
        public string TenantId { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Deposit { get; set; }
        public string? Terms { get; set; }
        public string? ContractFileUrl { get; set; }
        public string Status { get; set; } = null!;
        public DateOnly? ActualEndDate { get; set; }
        public string? HandoverNote { get; set; }
        public decimal DeductionAmount { get; set; }
        public string? DeductionReason { get; set; }
        public decimal RefundAmount { get; set; }
        public string? HandoverConfirmedBy { get; set; }
        public DateTime? HandoverConfirmedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ContractListResponse : PagedResponse<ContractResponse>
    {
    }
}
