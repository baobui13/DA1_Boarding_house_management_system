using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.DTOs.Notification.Requests;
using Backend_Boarding_house_management_system.DTOs.Notification.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using AutoMapper;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public NotificationService(INotificationRepository notificationRepository, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }

        public async Task<NotificationResponse> GetByIdAsync(GetNotificationByIdRequest request)
        {
            var entity = await _notificationRepository.GetByIdAsync(request.Id);
            if (entity == null)
            {
                throw new NotFoundException($"Không tìm thấy thông báo với Id '{request.Id}'.");
            }
            return _mapper.Map<NotificationResponse>(entity);
        }

        public async Task<NotificationListResponse> GetByFilterAsync(
            EntityFilter<Notification> filter,
            EntitySort<Notification> sort,
            EntityPage page)
        {
            var (items, totalCount) = await _notificationRepository.GetByFilterAsync(filter, sort, page);
            var response = new NotificationListResponse
            {
                Items = _mapper.Map<List<NotificationResponse>>(items),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
            return response;
        }

        public async Task<NotificationResponse> CreateAsync(CreateNotificationRequest request)
        {
            var entity = _mapper.Map<Notification>(request);
            entity.Id = Guid.NewGuid().ToString();
            entity.Timestamp = DateTime.UtcNow;
            entity.IsRead = false;

            await _notificationRepository.AddAsync(entity);

            var savedEntity = await _notificationRepository.GetByIdAsync(entity.Id);
            return _mapper.Map<NotificationResponse>(savedEntity);
        }

        public async Task UpdateAsync(UpdateNotificationRequest request)
        {
            var existing = await _notificationRepository.GetByIdAsync(request.Id);
            if (existing == null)
            {
                throw new NotFoundException($"Không tìm thấy thông báo với Id '{request.Id}'.");
            }

            existing.IsRead = request.IsRead;

            await _notificationRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(DeleteNotificationRequest request)
        {
            var exists = await _notificationRepository.ExistsAsync(request.Id);
            if (!exists)
            {
                throw new NotFoundException($"Không tìm thấy thông báo với Id '{request.Id}'.");
            }
            await _notificationRepository.DeleteAsync(request.Id);
        }
    }
}
