using Backend_Boarding_house_management_system.DTOs.Notification.Requests;
using Backend_Boarding_house_management_system.DTOs.Notification.Responses;
using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationResponse> GetByIdAsync(GetNotificationByIdRequest request);
        Task<NotificationListResponse> GetByFilterAsync(
            EntityFilter<Notification> filter,
            EntitySort<Notification> sort,
            EntityPage page);
        Task<NotificationResponse> CreateAsync(CreateNotificationRequest request);
        Task UpdateAsync(UpdateNotificationRequest request);
        Task DeleteAsync(DeleteNotificationRequest request);
    }
}
