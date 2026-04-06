using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Invoice.Requests
{
    public class CreateInvoiceRequest
    {
        [Required]
        public string ContractId { get; set; } = null!;

        [Required]
        public DateTime Period { get; set; }

        [Required]
        public decimal RentAmount { get; set; }

        public decimal? OldElectricityReading { get; set; }

        public decimal? NewElectricityReading { get; set; }

        public decimal? ElectricityCost { get; set; }

        public decimal? OldWaterReading { get; set; }

        public decimal? NewWaterReading { get; set; }

        public decimal? WaterCost { get; set; }

        public decimal? OtherFees { get; set; }

        public decimal Penalty { get; set; } = 0;

        [Required]
        public decimal Total { get; set; }

        public string? Note { get; set; }

        public string Status { get; set; } = "Pending";

        public string? InvoiceUrl { get; set; }

        public string? ReceiptUrl { get; set; }

        [Required]
        public DateTime DueDate { get; set; }
    }
}
