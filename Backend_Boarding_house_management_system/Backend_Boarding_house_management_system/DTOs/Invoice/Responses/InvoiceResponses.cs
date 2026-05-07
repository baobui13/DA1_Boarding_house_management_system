using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.Invoice.Responses
{
    using Backend_Boarding_house_management_system.DTOs.Contract.Responses;
    using Backend_Boarding_house_management_system.DTOs.Payment.Responses;
    using Backend_Boarding_house_management_system.DTOs.Property.Responses;
    using Backend_Boarding_house_management_system.DTOs.User.Responses;

    public class InvoiceResponse
    {
        public string Id { get; set; } = null!;
        public string ContractId { get; set; } = null!;
        public DateTime Period { get; set; }
        public decimal RentAmount { get; set; }
        public decimal? OldElectricityReading { get; set; }
        public decimal? NewElectricityReading { get; set; }
        public decimal? ElectricityCost { get; set; }
        public decimal? OldWaterReading { get; set; }
        public decimal? NewWaterReading { get; set; }
        public decimal? WaterCost { get; set; }
        public decimal? OtherFees { get; set; }
        public decimal Penalty { get; set; }
        public decimal Total { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = null!;
        public string? InvoiceUrl { get; set; }
        public string? ReceiptUrl { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class InvoiceListResponse : PagedResponse<InvoiceResponse>
    {
    }

    public class InvoiceDetailResponse : InvoiceResponse
    {
        public ContractResponse? Contract { get; set; }
        public PropertyResponse? Property { get; set; }
        public UserResponse? Tenant { get; set; }
        public List<PaymentResponse> Payments { get; set; } = new();
    }
}
