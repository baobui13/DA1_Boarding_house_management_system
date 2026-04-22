using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.DTOs.SearchHistory.Requests;
using Backend_Boarding_house_management_system.DTOs.SearchHistory.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using AutoMapper;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class SearchHistoryService : ISearchHistoryService
    {
        private readonly ISearchHistoryRepository _searchHistoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public SearchHistoryService(ISearchHistoryRepository searchHistoryRepository, IUserRepository userRepository, IMapper mapper)
        {
            _searchHistoryRepository = searchHistoryRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<SearchHistoryResponse> GetByIdAsync(string id)
        {
            var entity = await _searchHistoryRepository.GetByIdAsync(id);
            if (entity == null)
            {
                throw new NotFoundException($"Khong tim thay lich su tim kiem voi Id '{id}'.");
            }
            return _mapper.Map<SearchHistoryResponse>(entity);
        }

        public async Task<SearchHistoryListResponse> GetByFilterAsync(
            EntityFilter<SearchHistory> filter,
            EntitySort<SearchHistory> sort,
            EntityPage page)
        {
            var (items, totalCount) = await _searchHistoryRepository.GetByFilterAsync(filter, sort, page);
            var response = new SearchHistoryListResponse
            {
                Items = _mapper.Map<List<SearchHistoryResponse>>(items),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
            return response;
        }

        public async Task<SearchHistoryResponse> CreateAsync(CreateSearchHistoryRequest request)
        {
            if (await _userRepository.GetByIdAsync(request.UserId) == null)
                throw new NotFoundException($"Khong tim thay nguoi dung voi Id '{request.UserId}'.");

            var entity = _mapper.Map<SearchHistory>(request);
            entity.Id = Guid.NewGuid().ToString();
            entity.Timestamp = DateTime.UtcNow;

            await _searchHistoryRepository.AddAsync(entity);

            var savedEntity = await _searchHistoryRepository.GetByIdAsync(entity.Id);
            return _mapper.Map<SearchHistoryResponse>(savedEntity);
        }

        public async Task DeleteAsync(DeleteSearchHistoryRequest request)
        {
            var exists = await _searchHistoryRepository.ExistsAsync(request.Id);
            if (!exists)
            {
                throw new NotFoundException($"Khong tim thay lich su tim kiem voi Id '{request.Id}'.");
            }
            await _searchHistoryRepository.DeleteAsync(request.Id);
        }
    }
}
