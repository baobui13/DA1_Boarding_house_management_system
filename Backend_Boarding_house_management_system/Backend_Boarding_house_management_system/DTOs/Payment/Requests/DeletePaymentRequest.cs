using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Payment.Requests
{
    public class DeletePaymentRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
