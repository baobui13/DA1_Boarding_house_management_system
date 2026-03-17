using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Payment.Requests
{
    public class GetPaymentByIdRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
