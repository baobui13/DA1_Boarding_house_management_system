using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Invoice.Requests
{
    public class UpdateInvoiceRequest
    {
        [Required]
        public string Id { get; set; } = null!;

        public decimal? ElectricityUsage { get; set; }

        public decimal? ElectricityCost { get; set; }

        public decimal? WaterUsage { get; set; }

        public decimal? WaterCost { get; set; }

        public decimal? OtherFees { get; set; }

        public decimal? Penalty { get; set; }

        public decimal? Total { get; set; }

        public string? Note { get; set; }

        public string? Status { get; set; }

        public string? InvoiceUrl { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
