using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Contract.Requests
{
    public class CreateContractRequest
    {
        [Required]
        public string PropertyId { get; set; } = null!;

        [Required]
        public string TenantId { get; set; } = null!;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public decimal Deposit { get; set; }

        public string? Terms { get; set; }

        public string? ContractFileUrl { get; set; }

        public string Status { get; set; } = "Active";
    }
}
