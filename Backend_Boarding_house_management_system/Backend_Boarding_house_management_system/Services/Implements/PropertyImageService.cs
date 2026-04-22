using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.DTOs.PropertyImage.Requests;
using Backend_Boarding_house_management_system.DTOs.PropertyImage.Responses;
using Backend_Boarding_house_management_system.Exceptions;
using AutoMapper;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class PropertyImageService : IPropertyImageService
    {
        private readonly IPropertyImageRepository _propertyImageRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public PropertyImageService(
            IPropertyImageRepository propertyImageRepository,
            IPropertyRepository propertyRepository,
            IMapper mapper,
            IPhotoService photoService)
        {
            _propertyImageRepository = propertyImageRepository;
            _propertyRepository = propertyRepository;
            _mapper = mapper;
            _photoService = photoService;
        }

        public async Task<PropertyImageResponse> GetPropertyImageByIdAsync(GetPropertyImageByIdRequest request)
        {
            var image = await _propertyImageRepository.GetByIdAsync(request.Id);
            if (image == null)
                throw new NotFoundException($"Khong tim thay hinh anh voi Id '{request.Id}'.");

            return _mapper.Map<PropertyImageResponse>(image);
        }

        public async Task<PropertyImageDetailResponse> GetPropertyImageDetailByIdAsync(GetPropertyImageByIdRequest request)
        {
            var image = await _propertyImageRepository.GetByIdWithDetailsAsync(request.Id);
            if (image == null)
                throw new NotFoundException($"Khong tim thay hinh anh voi Id '{request.Id}'.");

            return _mapper.Map<PropertyImageDetailResponse>(image);
        }

        public async Task<PropertyImageListResponse> GetPropertyImagesByFilterAsync(
            EntityFilter<PropertyImage> filter,
            EntitySort<PropertyImage> sort,
            EntityPage page)
        {
            var (images, totalCount) = await _propertyImageRepository.GetByFilterAsync(filter, sort, page);
            return new PropertyImageListResponse
            {
                Items = _mapper.Map<List<PropertyImageResponse>>(images),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
        }

        public async Task<PropertyImageResponse> CreatePropertyImageAsync(CreatePropertyImageRequest request)
        {
            if (await _propertyRepository.GetByIdAsync(request.PropertyId) == null)
                throw new NotFoundException($"Khong tim thay phong voi Id '{request.PropertyId}'.");

            var uploadResult = await _photoService.AddPhotoAsync(request.File);
            if (uploadResult.Error != null)
                throw new BadRequestException("Upload anh len Cloudinary that bai: " + uploadResult.Error.Message);

            var entity = new PropertyImage
            {
                Id = Guid.NewGuid().ToString(),
                PropertyId = request.PropertyId,
                ImageUrl = uploadResult.SecureUrl.AbsoluteUri,
                PublicId = uploadResult.PublicId,
                IsPrimary = request.IsPrimary,
                CreatedAt = DateTime.UtcNow
            };

            await _propertyImageRepository.AddAsync(entity);
            var saved = await _propertyImageRepository.GetByIdAsync(entity.Id);
            return _mapper.Map<PropertyImageResponse>(saved ?? entity);
        }

        public async Task<bool> UpdatePropertyImageAsync(UpdatePropertyImageRequest request)
        {
            var image = await _propertyImageRepository.GetByIdAsync(request.Id);
            if (image == null)
                throw new NotFoundException($"Khong tim thay hinh anh voi Id '{request.Id}'.");

            if (request.IsPrimary.HasValue)
                image.IsPrimary = request.IsPrimary.Value;

            await _propertyImageRepository.UpdateAsync(image);
            return true;
        }

        public async Task<bool> DeletePropertyImageAsync(DeletePropertyImageRequest request)
        {
            if (!await _propertyImageRepository.ExistsAsync(request.Id))
                throw new NotFoundException($"Khong tim thay hinh anh voi Id '{request.Id}'.");

            await _propertyImageRepository.DeleteAsync(request.Id);
            return true;
        }
    }
}
