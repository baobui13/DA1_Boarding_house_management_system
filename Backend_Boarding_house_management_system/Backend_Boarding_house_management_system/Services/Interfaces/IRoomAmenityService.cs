using Backend_Boarding_house_management_system.DTOs.RoomAmenity.Requests;
using Backend_Boarding_house_management_system.DTOs.RoomAmenity.Responses;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IRoomAmenityService
    {
        Task<RoomAmenityResponse> GetByIdAsync(GetRoomAmenityByIdRequest request);
        Task<RoomAmenityListResponse> GetByFilterAsync(GetRoomAmenitiesByFilterRequest request);
        Task<RoomAmenityResponse> CreateAsync(CreateRoomAmenityRequest request);
        Task UpdateAsync(UpdateRoomAmenityRequest request);
        Task DeleteAsync(DeleteRoomAmenityRequest request);
    }
}
