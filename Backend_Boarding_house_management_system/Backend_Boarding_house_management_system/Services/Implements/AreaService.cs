using Backend_Boarding_house_management_system.DTOs.Area.Requests;
using Backend_Boarding_house_management_system.DTOs.Area.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Services.Interfaces;
using AutoMapper;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class AreaService : IAreaService
    {
        private readonly IAreaRepository _areaRepository;
        private readonly IMapper _mapper;
        public AreaService(IAreaRepository areaRepository, IMapper mapper)
        {
            _areaRepository = areaRepository;
            _mapper = mapper;
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
            return _mapper.Map<AreaResponse>(area);
        }

        public async Task<AreaListResponse> GetAreasByFilterAsync(GetAreasByFilterRequest request)
        {
            var (areas, totalCount) = await _areaRepository.GetAreasByFilterAsync(
                request.Name, request.Address, request.LandlordId, request.CreatedAfter, request.CreatedBefore,
                request.SortBy ?? "CreatedAt", request.IsDescending, request.PageNumber, request.PageSize);
            return new AreaListResponse
            {
                Items = _mapper.Map<List<AreaResponse>>(areas),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<AreaResponse> CreateAreaAsync(CreateAreaRequest request)
        {
            var area = _mapper.Map<Area>(request);
            area.Id = Guid.NewGuid().ToString();
            area.CreatedAt = DateTime.UtcNow;
            var created = await _areaRepository.CreateAreaAsync(area);
            if (created == null)
                throw new BadRequestException("Tạo khu vực thất bại.");
            return _mapper.Map<AreaResponse>(created);
        }

        public async Task<bool> UpdateAreaAsync(UpdateAreaRequest request)
        {
            var area = await _areaRepository.GetAreaByIdAsync(request.Id);
            if (area == null)
                throw new NotFoundException($"Không tìm thấy khu vực với Id '{request.Id}'.");
            _mapper.Map(request, area);
            var isSuccess = await _areaRepository.UpdateAreaAsync(area);
            if (!isSuccess)
                throw new BadRequestException("Cập nhật khu vực thất bại.");
            return true;
        }

        public async Task<bool> UpdateAreaDescriptionAsync(UpdateAreaDescriptionRequest request)
        {
            var area = await _areaRepository.GetAreaByIdAsync(request.Id);
            if (area == null)
                throw new NotFoundException($"Không tìm thấy khu vực với Id '{request.Id}'.");
            area.Description = request.Description;
            var isSuccess = await _areaRepository.UpdateAreaAsync(area);
            if (!isSuccess)
                throw new BadRequestException("Cập nhật mô tả khu vực thất bại.");
            return true;
        }

        public async Task<bool> DeleteAreaAsync(DeleteAreaRequest request)
        {
            var isSuccess = await _areaRepository.DeleteAreaAsync(request.Id);
            if (!isSuccess)
                throw new NotFoundException($"Không tìm thấy hoặc xóa khu vực thất bại với Id '{request.Id}'.");
            return true;
        }
    }
}
