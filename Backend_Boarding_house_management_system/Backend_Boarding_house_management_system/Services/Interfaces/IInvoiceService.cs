using Backend_Boarding_house_management_system.DTOs.Invoice.Requests;
using Backend_Boarding_house_management_system.DTOs.Invoice.Responses;
using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceResponse> GetByIdAsync(GetInvoiceByIdRequest request);
        Task<InvoiceListResponse> GetByFilterAsync(
            EntityFilter<Invoice> filter,
            EntitySort<Invoice> sort,
            EntityPage page);
        Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request);
        Task UpdateAsync(UpdateInvoiceRequest request);
        Task DeleteAsync(DeleteInvoiceRequest request);
    }
}
