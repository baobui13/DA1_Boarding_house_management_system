using Backend_Boarding_house_management_system.DTOs.Property.Requests;
using Backend_Boarding_house_management_system.DTOs.Property.Responses;
using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IPropertyService
    {
        Task<PropertyResponse> GetPropertyByIdAsync(GetPropertyByIdRequest request);
        Task<PropertyDetailResponse> GetPropertyDetailByIdAsync(GetPropertyByIdRequest request);
        Task<PropertyListResponse> GetModerationPropertiesAsync(GetModerationPropertiesRequest request);
        Task<PropertyListResponse> GetPropertiesByFilterAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page);

        /// <summary>
        /// Danh sách được ưu tiên đề cử theo lịch sử xem/tìm kiếm của user hiện tại (nếu đã đăng nhập).
        /// </summary>
        Task<PropertyListResponse> GetRecommendedPropertiesAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page);

        Task<PropertyResponse> CreatePropertyAsync(CreatePropertyRequest request);
        Task<bool> UpdatePropertyAsync(UpdatePropertyRequest request);
        Task<bool> ApprovePropertyAsync(ApprovePropertyRequest request);
        Task<bool> RejectPropertyAsync(RejectPropertyRequest request);
        Task<bool> UpdateAvailabilityStatusAsync(UpdateAvailabilityStatusRequest request);
        Task<bool> DeletePropertyAsync(DeletePropertyRequest request);
    }
}
