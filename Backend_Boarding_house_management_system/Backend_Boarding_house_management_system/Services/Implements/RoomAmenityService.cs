using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.DTOs.RoomAmenity.Requests;
using Backend_Boarding_house_management_system.DTOs.RoomAmenity.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using AutoMapper;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class RoomAmenityService : IRoomAmenityService
    {
        private readonly IRoomAmenityRepository _roomAmenityRepository;
        private readonly IMapper _mapper;

        public RoomAmenityService(IRoomAmenityRepository roomAmenityRepository, IMapper mapper)
        {
            _roomAmenityRepository = roomAmenityRepository;
            _mapper = mapper;
        }

        public async Task<RoomAmenityResponse> GetByIdAsync(GetRoomAmenityByIdRequest request)
        {
            var entity = await _roomAmenityRepository.GetByIdAsync(request.Id);
            if (entity == null)
            {
                throw new NotFoundException($"Không tìm thấy tiện ích phòng với Id '{request.Id}'.");
            }
            return _mapper.Map<RoomAmenityResponse>(entity);
        }

        public async Task<RoomAmenityListResponse> GetByFilterAsync(GetRoomAmenitiesByFilterRequest request)
        {
            var paged = await _roomAmenityRepository.GetByFilterAsync(request);
            var response = new RoomAmenityListResponse
            {
                Items = _mapper.Map<List<RoomAmenityResponse>>(paged.Items),
                TotalCount = paged.TotalCount,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize
            };
            return response;
        }

        public async Task<RoomAmenityResponse> CreateAsync(CreateRoomAmenityRequest request)
        {
            if (await _roomAmenityRepository.ExistsForRoomAndAmenityAsync(request.RoomId, request.AmenityId))
            {
                throw new ConflictException($"Tiện ích này đã được thêm vào phòng.");
            }

            var entity = _mapper.Map<RoomAmenity>(request);
            entity.Id = Guid.NewGuid().ToString();
            
            await _roomAmenityRepository.AddAsync(entity);
            
            // Reload to get AmenityName if possible, though mapper might not have it unless fetched
            var savedEntity = await _roomAmenityRepository.GetByIdAsync(entity.Id);
            return _mapper.Map<RoomAmenityResponse>(savedEntity);
        }

        public async Task UpdateAsync(UpdateRoomAmenityRequest request)
        {
            var existing = await _roomAmenityRepository.GetByIdAsync(request.Id);
            if (existing == null)
            {
                throw new NotFoundException($"Không tìm thấy tiện ích phòng với Id '{request.Id}'.");
            }

            _mapper.Map(request, existing);
            await _roomAmenityRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(DeleteRoomAmenityRequest request)
        {
            var exists = await _roomAmenityRepository.ExistsAsync(request.Id);
            if (!exists)
            {
                throw new NotFoundException($"Không tìm thấy tiện ích phòng với Id '{request.Id}'.");
            }
            await _roomAmenityRepository.DeleteAsync(request.Id);
        }
    }
}
