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
            var property = await _propertyRepository.GetByIdAsync(request.Id);
            if (property == null)
                throw new NotFoundException("Khong tim thay bat dong san.");

            return _mapper.Map<PropertyResponse>(property);
        }

        public async Task<PropertyListResponse> GetPropertiesByFilterAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page)
        {
            var (properties, totalCount) = await _propertyRepository.GetByFilterAsync(filter, sort, page);
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

            await _propertyRepository.AddAsync(property);
            return _mapper.Map<PropertyResponse>(property);
        }

        public async Task<bool> UpdatePropertyAsync(UpdatePropertyRequest request)
        {
            var property = await _propertyRepository.GetByIdAsync(request.Id);
            if (property == null)
                throw new NotFoundException($"Khong tim thay bat dong san voi Id '{request.Id}'.");

            _mapper.Map(request, property);
            await _propertyRepository.UpdateAsync(property);
            return true;
        }

        public async Task<bool> DeletePropertyAsync(DeletePropertyRequest request)
        {
            if (!await _propertyRepository.ExistsAsync(request.Id))
                throw new NotFoundException($"Khong tim thay bat dong san voi Id '{request.Id}'.");

            await _propertyRepository.DeleteAsync(request.Id);
            return true;
        }
    }
}
