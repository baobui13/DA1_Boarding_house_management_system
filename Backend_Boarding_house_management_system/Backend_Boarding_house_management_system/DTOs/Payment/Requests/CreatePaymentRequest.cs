using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Payment.Requests
{
    public class CreatePaymentRequest
    {
        [Required]
        public string InvoiceId { get; set; } = null!;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Method { get; set; } = null!;

        [StringLength(255)]
        public string? Note { get; set; }
    }
}
