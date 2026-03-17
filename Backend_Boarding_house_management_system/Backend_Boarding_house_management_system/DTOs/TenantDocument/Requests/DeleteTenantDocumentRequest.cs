using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.TenantDocument.Requests
{
    public class DeleteTenantDocumentRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
