using Backend_Boarding_house_management_system.DTOs.Rating.Requests;
using Backend_Boarding_house_management_system.DTOs.Rating.Responses;
using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Page;
using Plainquire.Sort;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IRatingService
    {
        Task<RatingResponse> GetRatingByIdAsync(GetRatingByIdRequest request);
        Task<RatingDetailResponse> GetRatingDetailByIdAsync(GetRatingByIdRequest request);
        Task<RatingListResponse> GetRatingsByFilterAsync(
            EntityFilter<Rating> filter,
            EntitySort<Rating> sort,
            EntityPage page);
        Task<RatingResponse> CreateRatingAsync(CreateRatingRequest request);
        Task<bool> UpdateRatingAsync(UpdateRatingRequest request);
        Task<bool> DeleteRatingAsync(DeleteRatingRequest request);
    }
}
