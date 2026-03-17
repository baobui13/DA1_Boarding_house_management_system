using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.TenantDocument.Requests
{
    public class UpdateTenantDocumentRequest
    {
        [Required]
        public string Id { get; set; } = null!;

        public string? DocumentType { get; set; }

        public string? FileUrl { get; set; }

        public string? Note { get; set; }
    }
}
