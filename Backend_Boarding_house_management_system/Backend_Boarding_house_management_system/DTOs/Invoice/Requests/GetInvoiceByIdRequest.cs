using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Invoice.Requests
{
    public class GetInvoiceByIdRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
