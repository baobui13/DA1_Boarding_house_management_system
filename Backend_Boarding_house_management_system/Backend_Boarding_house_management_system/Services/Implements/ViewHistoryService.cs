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
        private readonly IMapper _mapper;

        public ViewHistoryService(IViewHistoryRepository viewHistoryRepository, IMapper mapper)
        {
            _viewHistoryRepository = viewHistoryRepository;
            _mapper = mapper;
        }

        public async Task<ViewHistoryResponse> GetByIdAsync(string id)
        {
            var entity = await _viewHistoryRepository.GetByIdAsync(id);
            if (entity == null)
            {
                throw new NotFoundException($"Không tìm thấy lịch sử xem phòng với Id '{id}'.");
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
                throw new NotFoundException($"Không tìm thấy lịch sử xem phòng với Id '{request.Id}'.");
            }
            await _viewHistoryRepository.DeleteAsync(request.Id);
        }
    }
}
