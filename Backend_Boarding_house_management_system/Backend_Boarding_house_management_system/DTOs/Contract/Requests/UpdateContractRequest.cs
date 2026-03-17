using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Contract.Requests
{
    public class UpdateContractRequest
    {
        [Required]
        public string Id { get; set; } = null!;

        public DateTime? EndDate { get; set; }

        public string? Terms { get; set; }

        public string? ContractFileUrl { get; set; }

        public string? Status { get; set; }

        public DateOnly? ActualEndDate { get; set; }

        public string? HandoverNote { get; set; }

        public decimal? DeductionAmount { get; set; }

        public string? DeductionReason { get; set; }

        public decimal? RefundAmount { get; set; }

        public string? HandoverConfirmedBy { get; set; }

        public DateTime? HandoverConfirmedAt { get; set; }
    }
}
