using Backend_Boarding_house_management_system.DTOs.RoomAmenity.Requests;
using Backend_Boarding_house_management_system.DTOs.RoomAmenity.Responses;
using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IRoomAmenityService
    {
        Task<RoomAmenityResponse> GetByIdAsync(GetRoomAmenityByIdRequest request);
        Task<RoomAmenityListResponse> GetByFilterAsync(
            EntityFilter<RoomAmenity> filter,
            EntitySort<RoomAmenity> sort,
            EntityPage page);
        Task<RoomAmenityResponse> CreateAsync(CreateRoomAmenityRequest request);
        Task UpdateAsync(UpdateRoomAmenityRequest request);
        Task DeleteAsync(DeleteRoomAmenityRequest request);
    }
}
