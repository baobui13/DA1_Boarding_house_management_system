using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Backend_Boarding_house_management_system.DTOs.PropertyImage.Requests;
using Backend_Boarding_house_management_system.DTOs.PropertyImage.Responses;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class PropertyImageService : IPropertyImageService
    {
        private readonly IPhotoService _photoService;
        private readonly IPropertyImageRepository _repository;

        public PropertyImageService(IPhotoService photoService, IPropertyImageRepository repository)
        {
            _photoService = photoService;
            _repository = repository;
        }

        public async Task<ImageResponse?> UploadAndSaveImageAsync(ImageUploadRequest request)
        {
            // 1. Upload lên Cloudinary
            var result = await _photoService.AddPhotoAsync(request.File);
            if (result.Error != null) return null; // Xử lý lỗi upload ở đây

            // 2. Map dữ liệu vào Entity
            var propertyImage = new PropertyImage
            {
                Id = Guid.NewGuid().ToString(), // Hoặc dùng auto-increment tùy DB của bạn
                PropertyId = request.PropertyId,
                ImageUrl = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId, // Lưu lại để xóa sau này
                IsPrimary = request.IsPrimary,
                CreatedAt = DateTime.UtcNow
            };

            // 3. Lưu vào DB
            await _repository.AddImageAsync(propertyImage);
            if (await _repository.SaveChangesAsync())
            {
                // 4. Trả về DTO
                return new ImageResponse
                {
                    Id = propertyImage.Id,
                    ImageUrl = propertyImage.ImageUrl,
                    IsPrimary = propertyImage.IsPrimary,
                    CreatedAt = propertyImage.CreatedAt
                };
            }

            return null;
        }
    }
}
