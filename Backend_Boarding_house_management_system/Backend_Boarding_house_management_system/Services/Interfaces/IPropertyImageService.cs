using Backend_Boarding_house_management_system.DTOs.PropertyImage.Requests;
using Backend_Boarding_house_management_system.DTOs.PropertyImage.Responses;
using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IPropertyImageService
    {
        Task<PropertyImageResponse> GetPropertyImageByIdAsync(GetPropertyImageByIdRequest request);
        Task<PropertyImageListResponse> GetPropertyImagesByFilterAsync(
            EntityFilter<PropertyImage> filter,
            EntitySort<PropertyImage> sort,
            EntityPage page);
        Task<PropertyImageResponse> CreatePropertyImageAsync(CreatePropertyImageRequest request);
        Task<bool> UpdatePropertyImageAsync(UpdatePropertyImageRequest request);
        Task<bool> DeletePropertyImageAsync(DeletePropertyImageRequest request);
    }
}
