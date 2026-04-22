using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.DTOs.RoomAmenity.Requests;
using Backend_Boarding_house_management_system.DTOs.RoomAmenity.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using AutoMapper;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class RoomAmenityService : IRoomAmenityService
    {
        private readonly IRoomAmenityRepository _roomAmenityRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IAmenityRepository _amenityRepository;
        private readonly IMapper _mapper;

        public RoomAmenityService(
            IRoomAmenityRepository roomAmenityRepository,
            IPropertyRepository propertyRepository,
            IAmenityRepository amenityRepository,
            IMapper mapper)
        {
            _roomAmenityRepository = roomAmenityRepository;
            _propertyRepository = propertyRepository;
            _amenityRepository = amenityRepository;
            _mapper = mapper;
        }

        public async Task<RoomAmenityResponse> GetByIdAsync(GetRoomAmenityByIdRequest request)
        {
            var entity = await _roomAmenityRepository.GetByIdAsync(request.Id);
            if (entity == null)
            {
                throw new NotFoundException($"Khong tim thay tien ich phong voi Id '{request.Id}'.");
            }
            return _mapper.Map<RoomAmenityResponse>(entity);
        }

        public async Task<RoomAmenityListResponse> GetByFilterAsync(
            EntityFilter<RoomAmenity> filter,
            EntitySort<RoomAmenity> sort,
            EntityPage page)
        {
            var (items, totalCount) = await _roomAmenityRepository.GetByFilterAsync(filter, sort, page);
            var response = new RoomAmenityListResponse
            {
                Items = _mapper.Map<List<RoomAmenityResponse>>(items),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
            return response;
        }

        public async Task<RoomAmenityResponse> CreateAsync(CreateRoomAmenityRequest request)
        {
            if (await _propertyRepository.GetByIdAsync(request.PropertyId) == null)
                throw new NotFoundException($"Khong tim thay phong voi Id '{request.PropertyId}'.");

            if (await _amenityRepository.GetByIdAsync(request.AmenityId) == null)
                throw new NotFoundException($"Khong tim thay tien ich voi Id '{request.AmenityId}'.");

            if (await _roomAmenityRepository.ExistsForPropertyAndAmenityAsync(request.PropertyId, request.AmenityId))
            {
                throw new ConflictException("Tien ich nay da duoc them vao phong.");
            }

            var entity = _mapper.Map<RoomAmenity>(request);
            entity.Id = Guid.NewGuid().ToString();

            await _roomAmenityRepository.AddAsync(entity);

            var savedEntity = await _roomAmenityRepository.GetByIdAsync(entity.Id);
            return _mapper.Map<RoomAmenityResponse>(savedEntity);
        }

        public async Task UpdateAsync(UpdateRoomAmenityRequest request)
        {
            var existing = await _roomAmenityRepository.GetByIdAsync(request.Id);
            if (existing == null)
            {
                throw new NotFoundException($"Khong tim thay tien ich phong voi Id '{request.Id}'.");
            }

            _mapper.Map(request, existing);
            await _roomAmenityRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(DeleteRoomAmenityRequest request)
        {
            var exists = await _roomAmenityRepository.ExistsAsync(request.Id);
            if (!exists)
            {
                throw new NotFoundException($"Khong tim thay tien ich phong voi Id '{request.Id}'.");
            }
            await _roomAmenityRepository.DeleteAsync(request.Id);
        }
    }
}
