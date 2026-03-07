using Backend_Boarding_house_management_system.DTOs.Area.Requests;
using Backend_Boarding_house_management_system.DTOs.Area.Responses;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IAreaService
    {
        Task<AreaResponse> GetAreaByIdAsync(GetAreaByIdRequest request);
        Task<AreaListResponse> GetAreasByFilterAsync(GetAreasByFilterRequest request);
        Task<AreaResponse> CreateAreaAsync(CreateAreaRequest request);
        Task<bool> UpdateAreaAsync(UpdateAreaRequest request);
        Task<bool> UpdateAreaDescriptionAsync(UpdateAreaDescriptionRequest request);
        Task<bool> DeleteAreaAsync(DeleteAreaRequest request);
    }
}
