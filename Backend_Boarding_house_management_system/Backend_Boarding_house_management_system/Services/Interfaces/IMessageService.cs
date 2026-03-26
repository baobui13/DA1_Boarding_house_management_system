using Backend_Boarding_house_management_system.DTOs.Message.Requests;
using Backend_Boarding_house_management_system.DTOs.Message.Responses;
using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IMessageService
    {
        Task<MessageResponse> GetByIdAsync(GetMessageByIdRequest request);
        Task<MessageListResponse> GetByFilterAsync(
            EntityFilter<Message> filter,
            EntitySort<Message> sort,
            EntityPage page);
        Task<MessageResponse> CreateAsync(CreateMessageRequest request);
        Task UpdateAsync(UpdateMessageRequest request);
        Task DeleteAsync(DeleteMessageRequest request);
    }
}
