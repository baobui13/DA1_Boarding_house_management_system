using Backend_Boarding_house_management_system.DTOs.SearchHistory.Requests;
using Backend_Boarding_house_management_system.DTOs.SearchHistory.Responses;
using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface ISearchHistoryService
    {
        Task<SearchHistoryResponse> GetByIdAsync(string id);
        Task<SearchHistoryListResponse> GetByFilterAsync(
            EntityFilter<SearchHistory> filter,
            EntitySort<SearchHistory> sort,
            EntityPage page);
        Task<SearchHistoryResponse> CreateAsync(CreateSearchHistoryRequest request);
        Task DeleteAsync(DeleteSearchHistoryRequest request);
    }
}
