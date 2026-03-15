using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Backend_Boarding_house_management_system.DTOs.PropertyImage.Requests;
using Backend_Boarding_house_management_system.DTOs.PropertyImage.Responses;
using AutoMapper;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class PropertyImageService : IPropertyImageService
    {
        private readonly IPropertyImageRepository _propertyImageRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public PropertyImageService(IPropertyImageRepository propertyImageRepository, IMapper mapper, IPhotoService photoService)
        {
            _propertyImageRepository = propertyImageRepository;
            _mapper = mapper;
            _photoService = photoService;
        }

        public async Task<PropertyImageResponse> GetPropertyImageByIdAsync(GetPropertyImageByIdRequest request)
        {
            var image = await _propertyImageRepository.GetPropertyImageByIdAsync(request.Id);
            if (image == null)
                throw new Exception("Không tìm thấy hình ảnh.");
            return _mapper.Map<PropertyImageResponse>(image);
        }

        public async Task<PropertyImageListResponse> GetPropertyImagesByFilterAsync(GetPropertyImagesByFilterRequest request)
        {
            var (images, totalCount) = await _propertyImageRepository.GetPropertyImagesByFilterAsync(
                request.PropertyId, request.IsPrimary, request.SortBy ?? "CreatedAt", request.IsDescending, request.PageNumber, request.PageSize);
            return new PropertyImageListResponse
            {
                Items = _mapper.Map<List<PropertyImageResponse>>(images),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PropertyImageResponse> CreatePropertyImageAsync(CreatePropertyImageRequest request)
        {
            // Upload file lên Cloudinary
            var uploadResult = await _photoService.AddPhotoAsync(request.File);
            if (uploadResult.Error != null)
                throw new Exception("Lỗi upload ảnh lên Cloudinary: " + uploadResult.Error.Message);

            var entity = new PropertyImage
            {
                Id = Guid.NewGuid().ToString(),
                PropertyId = request.PropertyId,
                ImageUrl = uploadResult.SecureUrl.AbsoluteUri,
                PublicId = uploadResult.PublicId,
                IsPrimary = request.IsPrimary,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _propertyImageRepository.CreatePropertyImageAsync(entity);
            return _mapper.Map<PropertyImageResponse>(result);
        }

        public async Task<bool> UpdatePropertyImageAsync(UpdatePropertyImageRequest request)
        {
            var image = await _propertyImageRepository.GetPropertyImageByIdAsync(request.Id);
            if (image == null) return false;
            if (request.IsPrimary.HasValue)
                image.IsPrimary = request.IsPrimary.Value;
            return await _propertyImageRepository.UpdatePropertyImageAsync(image);
        }

        public async Task<bool> DeletePropertyImageAsync(DeletePropertyImageRequest request)
        {
            return await _propertyImageRepository.DeletePropertyImageAsync(request.Id);
        }
    }
}
