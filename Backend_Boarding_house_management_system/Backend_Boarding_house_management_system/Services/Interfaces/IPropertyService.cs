using Backend_Boarding_house_management_system.DTOs.Property.Requests;
using Backend_Boarding_house_management_system.DTOs.Property.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Options;
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
            EntityPage page,
            IDictionary<ReviewAspect, double>? searchAspectBoosts = null,
            RecommendationMode recommendationMode = RecommendationMode.PersonalMatch);

        /// <summary>
        /// Danh sách được ưu tiên đề cử theo lịch sử xem/tìm kiếm của user hiện tại (nếu đã đăng nhập).
        /// Hỗ trợ thêm aspect boosts từ search hiện tại.
        /// </summary>
        Task<PropertyListResponse> GetRecommendedPropertiesAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page,
            IDictionary<ReviewAspect, double>? searchAspectBoosts = null,
            RecommendationMode recommendationMode = RecommendationMode.PersonalMatch);

        /// <summary>
        /// Danh sách phòng được xem nhiều nhất (global popularity từ ViewHistory, có thể filter thêm).
        /// </summary>
        Task<PropertyListResponse> GetMostViewedPropertiesAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page,
            IDictionary<ReviewAspect, double>? searchAspectBoosts = null,
            RecommendationMode recommendationMode = RecommendationMode.HighAspectQuality);

        /// <summary>
        /// Danh sách trending / được tìm kiếm nhiều nhất (dựa trên global SearchHistory + view signals).
        /// </summary>
        Task<PropertyListResponse> GetTrendingPropertiesAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page,
            IDictionary<ReviewAspect, double>? searchAspectBoosts = null,
            RecommendationMode recommendationMode = RecommendationMode.HighAspectQuality);

        /// <summary>
        /// Các khoảng giá phổ biến hiện nay (dựa trên supply Properties đã approved + available, optional demand từ searches).
        /// Trả về metadata thay vì full property list.
        /// </summary>
        Task<PopularPriceRangesResponse> GetPopularPriceRangesAsync();

        Task<PropertyResponse> CreatePropertyAsync(CreatePropertyRequest request);
        Task<bool> UpdatePropertyAsync(UpdatePropertyRequest request);
        Task<bool> ApprovePropertyAsync(ApprovePropertyRequest request);
        Task<bool> RejectPropertyAsync(RejectPropertyRequest request);
        Task<bool> UpdateAvailabilityStatusAsync(UpdateAvailabilityStatusRequest request);
        Task<bool> DeletePropertyAsync(DeletePropertyRequest request);
    }
}
