using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.DTOs.TenantDocument.Requests;
using Backend_Boarding_house_management_system.DTOs.Notification.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using AutoMapper;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;
using Backend_Boarding_house_management_system.DTOs.TenantDocument.Responses;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class TenantDocumentService : ITenantDocumentService
    {
        private readonly ITenantDocumentRepository _tenantDocumentRepository;
        private readonly IMapper _mapper;

        public TenantDocumentService(ITenantDocumentRepository tenantDocumentRepository, IMapper mapper)
        {
            _tenantDocumentRepository = tenantDocumentRepository;
            _mapper = mapper;
        }

        public async Task<TenantDocumentResponse> GetByIdAsync(GetTenantDocumentByIdRequest request)
        {
            var entity = await _tenantDocumentRepository.GetByIdAsync(request.Id);
            if (entity == null)
            {
                throw new NotFoundException($"Không tìm thấy tài liệu với Id '{request.Id}'.");
            }
            return _mapper.Map<TenantDocumentResponse>(entity);
        }

        public async Task<TenantDocumentListResponse> GetByFilterAsync(
            EntityFilter<TenantDocument> filter,
            EntitySort<TenantDocument> sort,
            EntityPage page)
        {
            var (items, totalCount) = await _tenantDocumentRepository.GetByFilterAsync(filter, sort, page);
            var response = new TenantDocumentListResponse
            {
                Items = _mapper.Map<List<TenantDocumentResponse>>(items),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
            return response;
        }

        public async Task<TenantDocumentResponse> CreateAsync(CreateTenantDocumentRequest request)
        {
            var entity = _mapper.Map<TenantDocument>(request);
            entity.Id = Guid.NewGuid().ToString();
            entity.CreatedAt = DateTime.UtcNow;

            await _tenantDocumentRepository.AddAsync(entity);

            var savedEntity = await _tenantDocumentRepository.GetByIdAsync(entity.Id);
            return _mapper.Map<TenantDocumentResponse>(savedEntity);
        }

        public async Task UpdateAsync(UpdateTenantDocumentRequest request)
        {
            var existing = await _tenantDocumentRepository.GetByIdAsync(request.Id);
            if (existing == null)
            {
                throw new NotFoundException($"Không tìm thấy tài liệu với Id '{request.Id}'.");
            }

            _mapper.Map(request, existing);

            await _tenantDocumentRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(DeleteTenantDocumentRequest request)
        {
            var exists = await _tenantDocumentRepository.ExistsAsync(request.Id);
            if (!exists)
            {
                throw new NotFoundException($"Không tìm thấy tài liệu với Id '{request.Id}'.");
            }
            await _tenantDocumentRepository.DeleteAsync(request.Id);
        }
    }
}
