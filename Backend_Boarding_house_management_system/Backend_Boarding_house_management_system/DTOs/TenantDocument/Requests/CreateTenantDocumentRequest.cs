using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.TenantDocument.Requests
{
    public class CreateTenantDocumentRequest
    {
        [Required]
        public string TenantId { get; set; } = null!;

        [Required]
        public string DocumentType { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string FileUrl { get; set; } = null!;

        [StringLength(255)]
        public string? Note { get; set; }
    }
}
