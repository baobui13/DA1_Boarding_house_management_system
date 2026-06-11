using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.DTOs.Amenity.Requests;
using Backend_Boarding_house_management_system.DTOs.Amenity.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using AutoMapper;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class AmenityService : IAmenityService
    {
        private readonly IAmenityRepository _amenityRepository;
        private readonly IMapper _mapper;
        public AmenityService(IAmenityRepository amenityRepository, IMapper mapper)
        {
            _amenityRepository = amenityRepository;
            _mapper = mapper;
        }

        public async Task<AmenityResponse> GetByIdAsync(string id)
        {
            var amenity = await _amenityRepository.GetByIdAsync(id);
            if (amenity == null)
            {
                throw new NotFoundException($"Không tìm thấy tiện ích với Id '{id}'.");
            }
            return _mapper.Map<AmenityResponse>(amenity);
        }

        public async Task<AmenityPagedResponse> GetByFilterAsync(
            EntityFilter<Amenity> filter,
            EntitySort<Amenity> sort,
            EntityPage page)
        {
            var (items, totalCount) = await _amenityRepository.GetByFilterAsync(filter, sort, page);
            var response = new AmenityPagedResponse
            {
                Items = _mapper.Map<List<AmenityResponse>>(items),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
            return response;
        }

        public async Task<List<AmenityResponse>> GetAllAsync()
        {
            var amenities = await _amenityRepository.GetAllAsync();
            return _mapper.Map<List<AmenityResponse>>(amenities);
        }

        public async Task AddAsync(CreateAmenityRequest request)
        {
            var amenity = _mapper.Map<Amenity>(request);
            amenity.Id = Guid.NewGuid().ToString();
            if (await _amenityRepository.ExistsAsync(amenity.Id))
            {
                throw new ConflictException("Khong the tao tien ich.");
            }
            await _amenityRepository.AddAsync(amenity);
        }

        public async Task UpdateAsync(UpdateAmenityRequest request)
        {
            var existing = await _amenityRepository.GetByIdAsync(request.Id);
            if (existing == null)
            {
                throw new NotFoundException($"Không tìm thấy tiện ích với Id '{request.Id}'.");
            }
            _mapper.Map(request, existing);
            await _amenityRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(string id)
        {
            var exists = await _amenityRepository.ExistsAsync(id);
            if (!exists)
            {
                throw new NotFoundException($"Không tìm thấy tiện ích với Id '{id}'.");
            }
            await _amenityRepository.DeleteAsync(id);
        }
    }
}
