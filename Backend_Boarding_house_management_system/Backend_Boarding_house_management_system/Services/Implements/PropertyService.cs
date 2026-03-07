using Backend_Boarding_house_management_system.DTOs.Property.Requests;
using Backend_Boarding_house_management_system.DTOs.Property.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Services.Interfaces;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class PropertyService : IPropertyService
    {
        private readonly IPropertyRepository _propertyRepository;
        public PropertyService(IPropertyRepository propertyRepository)
        {
            _propertyRepository = propertyRepository;
        }

        public async Task<PropertyResponse> GetPropertyByIdAsync(GetPropertyByIdRequest request)
        {
            var property = await _propertyRepository.GetPropertyByIdAsync(request.Id);
            if (property == null)
                throw new NotFoundException("Không tìm thấy bất động sản.");
            return MapToResponse(property);
        }

        public async Task<PropertyListResponse> GetPropertiesByFilterAsync(GetPropertiesByFilterRequest request)
        {
            var (properties, totalCount) = await _propertyRepository.GetPropertiesByFilterAsync(
                request.LandlordId, request.AreaId, request.PropertyName, request.Address, request.MinPrice, request.MaxPrice, request.Status, request.MinSize, request.MaxSize, request.CreatedAfter, request.CreatedBefore,
                request.SortBy ?? "CreatedAt", request.IsDescending, request.PageNumber, request.PageSize);
            return new PropertyListResponse
            {
                Items = properties.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PropertyResponse> CreatePropertyAsync(CreatePropertyRequest request)
        {
            var property = new Property
            {
                Id = Guid.NewGuid().ToString(),
                LandlordId = request.LandlordId,
                AreaId = request.AreaId,
                PropertyName = request.PropertyName,
                Address = request.Address,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Size = request.Size,
                Description = request.Description,
                Price = request.Price,
                Status = request.Status,
                CreatedAt = DateTime.UtcNow
            };
            var created = await _propertyRepository.CreatePropertyAsync(property);
            if (created == null)
                throw new BadRequestException("Tạo bất động sản thất bại.");
            return MapToResponse(created);
        }

        public async Task<bool> UpdatePropertyAsync(UpdatePropertyRequest request)
        {
            var property = await _propertyRepository.GetPropertyByIdAsync(request.Id);
            if (property == null)
                throw new NotFoundException($"Không tìm thấy bất động sản với Id '{request.Id}'.");

            if (request.PropertyName != null) 
                property.PropertyName = request.PropertyName;

            if (request.Address != null) 
                property.Address = request.Address;

            if (request.Latitude.HasValue) 
                property.Latitude = request.Latitude;

            if (request.Longitude.HasValue) 
                property.Longitude = request.Longitude;

            if (request.Size.HasValue) 
                property.Size = request.Size.Value;

            if (request.Description != null) 
                property.Description = request.Description;

            if (request.Price.HasValue) 
                property.Price = request.Price.Value;

            if (request.Status != null)
                property.Status = request.Status;

            if (request.AreaId != null) 
                property.AreaId = request.AreaId;

            if (request.RejectionReason != null) 
                property.RejectionReason = request.RejectionReason;

            var isSuccess = await _propertyRepository.UpdatePropertyAsync(property);
            if (!isSuccess)
                throw new BadRequestException("Cập nhật bất động sản thất bại.");
            return true;
        }

        public async Task<bool> DeletePropertyAsync(DeletePropertyRequest request)
        {
            var isSuccess = await _propertyRepository.DeletePropertyAsync(request.Id);
            if (!isSuccess)
                throw new NotFoundException($"Không tìm thấy hoặc xóa bất động sản thất bại với Id '{request.Id}'.");
            return true;
        }

        private static PropertyResponse MapToResponse(Property property)
        {
            return new PropertyResponse
            {
                Id = property.Id,
                LandlordId = property.LandlordId,
                AreaId = property.AreaId,
                PropertyName = property.PropertyName,
                Address = property.Address,
                Latitude = property.Latitude,
                Longitude = property.Longitude,
                Size = property.Size,
                Description = property.Description,
                Price = property.Price,
                Status = property.Status,
                RejectionReason = property.RejectionReason,
                CreatedAt = property.CreatedAt,
                UpdatedAt = property.UpdatedAt
            };
        }
    }
}
