using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.TenantDocument.Requests
{
    public class GetTenantDocumentsByFilterRequest : PagedRequest
    {
        public string? TenantId { get; set; }
        public string? DocumentType { get; set; }
    }
}
