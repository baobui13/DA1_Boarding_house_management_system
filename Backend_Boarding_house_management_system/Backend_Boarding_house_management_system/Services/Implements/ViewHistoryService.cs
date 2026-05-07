using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.DTOs.ViewHistory.Requests;
using Backend_Boarding_house_management_system.DTOs.ViewHistory.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using AutoMapper;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class ViewHistoryService : IViewHistoryService
    {
        private readonly IViewHistoryRepository _viewHistoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;

        public ViewHistoryService(
            IViewHistoryRepository viewHistoryRepository,
            IUserRepository userRepository,
            IPropertyRepository propertyRepository,
            IMapper mapper)
        {
            _viewHistoryRepository = viewHistoryRepository;
            _userRepository = userRepository;
            _propertyRepository = propertyRepository;
            _mapper = mapper;
        }

        public async Task<ViewHistoryResponse> GetByIdAsync(string id)
        {
            var entity = await _viewHistoryRepository.GetByIdAsync(id);
            if (entity == null)
            {
                throw new NotFoundException($"Khong tim thay lich su xem phong voi Id '{id}'.");
            }
            return _mapper.Map<ViewHistoryResponse>(entity);
        }

        public async Task<ViewHistoryListResponse> GetByFilterAsync(
            EntityFilter<ViewHistory> filter,
            EntitySort<ViewHistory> sort,
            EntityPage page)
        {
            var (items, totalCount) = await _viewHistoryRepository.GetByFilterAsync(filter, sort, page);
            var response = new ViewHistoryListResponse
            {
                Items = _mapper.Map<List<ViewHistoryResponse>>(items),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
            return response;
        }

        public async Task<ViewHistoryResponse> CreateAsync(CreateViewHistoryRequest request)
        {
            if (await _userRepository.GetByIdAsync(request.UserId) == null)
                throw new NotFoundException($"Khong tim thay nguoi dung voi Id '{request.UserId}'.");

            if (await _propertyRepository.GetByIdAsync(request.PropertyId) == null)
                throw new NotFoundException($"Khong tim thay phong voi Id '{request.PropertyId}'.");

            var entity = _mapper.Map<ViewHistory>(request);
            entity.Id = Guid.NewGuid().ToString();
            entity.Timestamp = DateTime.UtcNow;

            await _viewHistoryRepository.AddAsync(entity);

            var savedEntity = await _viewHistoryRepository.GetByIdAsync(entity.Id);
            return _mapper.Map<ViewHistoryResponse>(savedEntity);
        }

        public async Task DeleteAsync(DeleteViewHistoryRequest request)
        {
            var exists = await _viewHistoryRepository.ExistsAsync(request.Id);
            if (!exists)
            {
                throw new NotFoundException($"Khong tim thay lich su xem phong voi Id '{request.Id}'.");
            }
            await _viewHistoryRepository.DeleteAsync(request.Id);
        }
    }
}
