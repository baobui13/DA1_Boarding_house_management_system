using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces.Base;
using Plainquire.Filter;
using Plainquire.Page;
using Plainquire.Sort;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces
{
    public interface IPropertyRepository : IRepository<Property, string>
    {
        Task<Property?> GetByIdWithDetailsAsync(string id);
        Task<(IEnumerable<Property> Items, int TotalCount)> GetByFilterWithDetailsAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page);

        /// <summary>
        /// Lấy danh sách property match filter (kèm RoomAmenities) giới hạn số lượng để scoring recommendation.
        /// Không áp dụng page/sort của client (sẽ re-rank ở service).
        /// </summary>
        Task<IEnumerable<Property>> GetFilteredCandidatesForRecAsync(EntityFilter<Property> filter, int maxCandidates = 200);

        /// <summary>
        /// Lấy top propertyIds theo số lượt xem (ViewHistory) trong phạm vi filter, giới hạn để tránh heavy query.
        /// Trả về map PropertyId -> ViewCount (sắp xếp sẵn theo count desc).
        /// </summary>
        Task<Dictionary<string, int>> GetPropertyViewCountsAsync(EntityFilter<Property> filter, int maxCandidates = 300);

        /// <summary>
        /// Lấy distribution giá (bucket counts) cho các property match filter (thường Approved + Available).
        /// </summary>
        Task<Dictionary<string, int>> GetPriceBucketCountsAsync(EntityFilter<Property> filter, List<(string Label, decimal Min, decimal Max)> buckets);
    }
}
