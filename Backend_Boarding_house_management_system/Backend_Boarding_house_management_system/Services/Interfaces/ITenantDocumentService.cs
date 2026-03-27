using Backend_Boarding_house_management_system.DTOs.TenantDocument.Requests;
using Backend_Boarding_house_management_system.DTOs.TenantDocument.Responses;
using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface ITenantDocumentService
    {
        Task<TenantDocumentResponse> GetByIdAsync(GetTenantDocumentByIdRequest request);
        Task<TenantDocumentListResponse> GetByFilterAsync(
            EntityFilter<TenantDocument> filter,
            EntitySort<TenantDocument> sort,
            EntityPage page);
        Task<TenantDocumentResponse> CreateAsync(CreateTenantDocumentRequest request);
        Task UpdateAsync(UpdateTenantDocumentRequest request);
        Task DeleteAsync(DeleteTenantDocumentRequest request);
    }
}
