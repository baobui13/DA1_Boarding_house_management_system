using Backend_Boarding_house_management_system.DTOs.Area.Requests;
using Backend_Boarding_house_management_system.DTOs.Area.Responses;
using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IAreaService
    {
        Task<AreaResponse> GetAreaByIdAsync(GetAreaByIdRequest request);
        Task<AreaDetailResponse> GetAreaDetailByIdAsync(GetAreaByIdRequest request);
        Task<AreaListResponse> GetAreasByFilterAsync(
            EntityFilter<Area> filter,
            EntitySort<Area> sort,
            EntityPage page);
        Task<AreaResponse> CreateAreaAsync(CreateAreaRequest request);
        Task<bool> UpdateAreaAsync(UpdateAreaRequest request);
        Task<bool> UpdateAreaDescriptionAsync(UpdateAreaDescriptionRequest request);
        Task<bool> DeleteAreaAsync(DeleteAreaRequest request);
    }
}
