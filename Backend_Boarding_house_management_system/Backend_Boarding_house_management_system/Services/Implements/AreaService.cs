using Backend_Boarding_house_management_system.DTOs.Area.Requests;
using Backend_Boarding_house_management_system.DTOs.Area.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Services.Interfaces;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class AreaService : IAreaService
    {
        private readonly IAreaRepository _areaRepository;
        public AreaService(IAreaRepository areaRepository)
        {
            _areaRepository = areaRepository;
        }

        public async Task<AreaResponse> GetAreaByIdAsync(GetAreaByIdRequest request)
        {
            Area? area = null;
            if (!string.IsNullOrEmpty(request.Id))
                area = await _areaRepository.GetAreaByIdAsync(request.Id);
            else
                throw new BadRequestException("Phải cung cấp Id.");

            if (area == null)
                throw new NotFoundException("Không tìm thấy khu vực.");
            return MapToResponse(area);
        }

        public async Task<AreaListResponse> GetAreasByFilterAsync(GetAreasByFilterRequest request)
        {
            var (areas, totalCount) = await _areaRepository.GetAreasByFilterAsync(
                request.Name, request.Address, request.LandlordId, request.CreatedAfter, request.CreatedBefore,
                request.SortBy ?? "CreatedAt", request.IsDescending, request.PageNumber, request.PageSize);
            return new AreaListResponse
            {
                Items = areas.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<AreaResponse> CreateAreaAsync(CreateAreaRequest request)
        {
            var area = new Area
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Address = request.Address,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                RoomCount = request.RoomCount,
                Description = request.Description,
                LandlordId = request.LandlordId,
                CreatedAt = DateTime.UtcNow
            };
            var created = await _areaRepository.CreateAreaAsync(area);
            return MapToResponse(created);
        }

        public async Task<bool> UpdateAreaAsync(UpdateAreaRequest request)
        {
            var area = await _areaRepository.GetAreaByIdAsync(request.Id);
            if (request.Name != null) area.Name = request.Name;
            if (request.Address != null) area.Address = request.Address;
            if (request.Latitude.HasValue) area.Latitude = request.Latitude;
            if (request.Longitude.HasValue) area.Longitude = request.Longitude;
            if (request.RoomCount.HasValue) area.RoomCount = request.RoomCount.Value;
            if (request.Description != null) area.Description = request.Description;
            return await _areaRepository.UpdateAreaAsync(area);
        }

        public async Task<bool> UpdateAreaDescriptionAsync(UpdateAreaDescriptionRequest request)
        {
            var area = await _areaRepository.GetAreaByIdAsync(request.Id);
            area.Description = request.Description;
            return await _areaRepository.UpdateAreaAsync(area);
        }

        public async Task<bool> DeleteAreaAsync(DeleteAreaRequest request)
        {
            return await _areaRepository.DeleteAreaAsync(request.Id);
        }

        private static AreaResponse MapToResponse(Area area)
        {
            return new AreaResponse
            {
                Id = area.Id,
                Name = area.Name,
                Address = area.Address,
                Latitude = area.Latitude,
                Longitude = area.Longitude,
                RoomCount = area.RoomCount,
                Description = area.Description,
                LandlordId = area.LandlordId,
                CreatedAt = area.CreatedAt,
                UpdatedAt = area.UpdatedAt
            };
        }
    }
}
