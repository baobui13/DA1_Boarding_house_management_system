using System.Threading.Tasks;
using Backend_Boarding_house_management_system.DTOs.PropertyImage.Requests;
using Backend_Boarding_house_management_system.DTOs.PropertyImage.Responses;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IPropertyImageService
    {
        Task<PropertyImageResponse> GetPropertyImageByIdAsync(GetPropertyImageByIdRequest request);
        Task<PropertyImageListResponse> GetPropertyImagesByFilterAsync(GetPropertyImagesByFilterRequest request);
        Task<PropertyImageResponse> CreatePropertyImageAsync(CreatePropertyImageRequest request);
        Task<bool> UpdatePropertyImageAsync(UpdatePropertyImageRequest request);
        Task<bool> DeletePropertyImageAsync(DeletePropertyImageRequest request);
    }
}
