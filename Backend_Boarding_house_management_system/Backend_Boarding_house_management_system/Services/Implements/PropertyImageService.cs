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
                throw new NotFoundException($"Khong tim thay bat dong san voi Id '{request.PropertyId}'.");

            // Basic file validation
            if (request.File == null || request.File.Length == 0)
                throw new BadRequestException("File anh khong hop le.");
            const long maxSize = 10 * 1024 * 1024; // 10MB
            if (request.File.Length > maxSize)
                throw new BadRequestException("Kich thuoc anh vuot qua 10MB. Vui long chon anh nho hon.");
            var contentType = request.File.ContentType?.ToLowerInvariant() ?? string.Empty;
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif" };
            if (!allowedTypes.Contains(contentType))
                throw new BadRequestException("Chi ho tro cac loai file anh: JPEG, PNG, WEBP, GIF.");

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

            string? uploadedPublicId = uploadResult.PublicId;

            try
            {
                if (entity.IsPrimary)
                {
                    await UnsetOtherPrimariesAsync(request.PropertyId);
                }

                await _propertyImageRepository.AddAsync(entity);
            }
            catch
            {
                // Best-effort cleanup of uploaded photo if DB save fails
                if (!string.IsNullOrWhiteSpace(uploadedPublicId))
                {
                    try { await _photoService.DeletePhotoAsync(uploadedPublicId); } catch { /* ignore */ }
                }
                throw;
            }

            var saved = await _propertyImageRepository.GetByIdAsync(entity.Id);
            return _mapper.Map<PropertyImageResponse>(saved ?? entity);
        }

        public async Task<bool> UpdatePropertyImageAsync(UpdatePropertyImageRequest request)
        {
            var image = await _propertyImageRepository.GetByIdAsync(request.Id);
            if (image == null)
                throw new NotFoundException($"Khong tim thay hinh anh voi Id '{request.Id}'.");

            if (request.IsPrimary.HasValue)
            {
                if (request.IsPrimary.Value && !image.IsPrimary)
                {
                    await UnsetOtherPrimariesAsync(image.PropertyId, image.Id);
                    image.IsPrimary = true;
                }
                else
                {
                    image.IsPrimary = request.IsPrimary.Value;
                }
            }

            await _propertyImageRepository.UpdateAsync(image);
            return true;
        }

        public async Task<PropertyImageResponse> ReplacePropertyImageAsync(ReplacePropertyImageRequest request)
        {
            var image = await _propertyImageRepository.GetByIdAsync(request.Id);
            if (image == null)
                throw new NotFoundException($"Khong tim thay hinh anh voi Id '{request.Id}'.");

            // Validate new file (same rules as create)
            if (request.File == null || request.File.Length == 0)
                throw new BadRequestException("File anh khong hop le.");
            const long maxSize = 10 * 1024 * 1024;
            if (request.File.Length > maxSize)
                throw new BadRequestException("Kich thuoc anh vuot qua 10MB. Vui long chon anh nho hon.");
            var contentType = request.File.ContentType?.ToLowerInvariant() ?? string.Empty;
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif" };
            if (!allowedTypes.Contains(contentType))
                throw new BadRequestException("Chi ho tro cac loai file anh: JPEG, PNG, WEBP, GIF.");

            var uploadResult = await _photoService.AddPhotoAsync(request.File);
            if (uploadResult.Error != null)
                throw new BadRequestException("Upload anh len Cloudinary that bai: " + uploadResult.Error.Message);

            var newPublicId = uploadResult.PublicId;
            var newUrl = uploadResult.SecureUrl.AbsoluteUri;

            var oldPublicId = image.PublicId;

            try
            {
                // Update entity
                image.ImageUrl = newUrl;
                image.PublicId = newPublicId;

                await _propertyImageRepository.UpdateAsync(image);

                // After successful DB update, remove the old photo from Cloudinary (best effort)
                if (!string.IsNullOrWhiteSpace(oldPublicId) && oldPublicId != newPublicId)
                {
                    try { await _photoService.DeletePhotoAsync(oldPublicId); } catch { /* ignore */ }
                }
            }
            catch
            {
                // Rollback the newly uploaded photo if DB update failed
                if (!string.IsNullOrWhiteSpace(newPublicId))
                {
                    try { await _photoService.DeletePhotoAsync(newPublicId); } catch { /* ignore */ }
                }
                throw;
            }

            var refreshed = await _propertyImageRepository.GetByIdAsync(image.Id);
            return _mapper.Map<PropertyImageResponse>(refreshed ?? image);
        }

        public async Task<List<PropertyImageResponse>> GetByPropertyIdAsync(string propertyId)
        {
            var entities = await _propertyImageRepository.GetByPropertyIdAsync(propertyId);
            return _mapper.Map<List<PropertyImageResponse>>(entities);
        }

        public async Task<bool> DeletePropertyImageAsync(DeletePropertyImageRequest request)
        {
            var image = await _propertyImageRepository.GetByIdAsync(request.Id);
            if (image == null)
                throw new NotFoundException($"Khong tim thay hinh anh voi Id '{request.Id}'.");

            var wasPrimary = image.IsPrimary;
            var propertyId = image.PropertyId;
            var publicId = image.PublicId;

            // Delete from Cloudinary first (best effort - proceed even if fails to avoid stuck DB records)
            if (!string.IsNullOrWhiteSpace(publicId))
            {
                try
                {
                    await _photoService.DeletePhotoAsync(publicId);
                }
                catch
                {
                    // Best effort: Cloudinary delete failure should not block removal from DB
                }
            }

            await _propertyImageRepository.DeleteAsync(request.Id);

            // If we deleted the primary image, promote another image (oldest) to primary if any remain
            if (wasPrimary)
            {
                var remaining = (await _propertyImageRepository.GetByPropertyIdAsync(propertyId))
                    .OrderBy(x => x.CreatedAt)
                    .FirstOrDefault();

                if (remaining != null && !remaining.IsPrimary)
                {
                    remaining.IsPrimary = true;
                    await _propertyImageRepository.UpdateAsync(remaining);
                }
            }

            return true;
        }

        private async Task UnsetOtherPrimariesAsync(string propertyId, string? excludeId = null)
        {
            var images = await _propertyImageRepository.GetByPropertyIdAsync(propertyId);
            foreach (var img in images)
            {
                if (img.IsPrimary && img.Id != excludeId)
                {
                    img.IsPrimary = false;
                    await _propertyImageRepository.UpdateAsync(img);
                }
            }
        }
    }
}
