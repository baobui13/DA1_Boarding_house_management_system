using Backend_Boarding_house_management_system.DTOs.ViewHistory.Requests;
using Backend_Boarding_house_management_system.DTOs.ViewHistory.Responses;
using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IViewHistoryService
    {
        Task<ViewHistoryResponse> GetByIdAsync(string id);
        Task<ViewHistoryListResponse> GetByFilterAsync(
            EntityFilter<ViewHistory> filter,
            EntitySort<ViewHistory> sort,
            EntityPage page);
        Task<ViewHistoryResponse> CreateAsync(CreateViewHistoryRequest request);
        Task DeleteAsync(DeleteViewHistoryRequest request);
    }
}
