using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.TenantDocument.Responses
{
    public class TenantDocumentResponse
    {
        public string Id { get; set; } = null!;
        public string TenantId { get; set; } = null!;
        public string DocumentType { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TenantDocumentListResponse : PagedResponse<TenantDocumentResponse>
    {
    }
}
