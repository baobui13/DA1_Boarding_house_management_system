using Backend_Boarding_house_management_system.DTOs.Property.Requests;
using Backend_Boarding_house_management_system.DTOs.Property.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Services.Interfaces;
using AutoMapper;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class PropertyService : IPropertyService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;
        public PropertyService(IPropertyRepository propertyRepository, IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _mapper = mapper;
        }

        public async Task<PropertyResponse> GetPropertyByIdAsync(GetPropertyByIdRequest request)
        {
            var property = await _propertyRepository.GetPropertyByIdAsync(request.Id);
            if (property == null)
                throw new NotFoundException("Không tìm thấy bất động sản.");
            return _mapper.Map<PropertyResponse>(property);
        }

        public async Task<PropertyListResponse> GetPropertiesByFilterAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page)
        {
            var (properties, totalCount) = await _propertyRepository.GetPropertiesByFilterAsync(filter, sort, page);
            return new PropertyListResponse
            {
                Items = _mapper.Map<List<PropertyResponse>>(properties),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
        }

        public async Task<PropertyResponse> CreatePropertyAsync(CreatePropertyRequest request)
        {
            var property = _mapper.Map<Property>(request);
            property.Id = Guid.NewGuid().ToString();
            property.CreatedAt = DateTime.UtcNow;
            var created = await _propertyRepository.CreatePropertyAsync(property);
            if (created == null)
                throw new BadRequestException("Tạo bất động sản thất bại.");
            return _mapper.Map<PropertyResponse>(created);
        }

        public async Task<bool> UpdatePropertyAsync(UpdatePropertyRequest request)
        {
            var property = await _propertyRepository.GetPropertyByIdAsync(request.Id);
            if (property == null)
                throw new NotFoundException($"Không tìm thấy bất động sản với Id '{request.Id}'.");
            _mapper.Map(request, property);
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
    }
}
