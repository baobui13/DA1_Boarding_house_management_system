using Backend_Boarding_house_management_system.Services.Interfaces;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.DTOs.Message.Requests;
using Backend_Boarding_house_management_system.DTOs.Message.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using AutoMapper;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public MessageService(IMessageRepository messageRepository, IMapper mapper)
        {
            _messageRepository = messageRepository;
            _mapper = mapper;
        }

        public async Task<MessageResponse> GetByIdAsync(GetMessageByIdRequest request)
        {
            var entity = await _messageRepository.GetByIdAsync(request.Id);
            if (entity == null)
            {
                throw new NotFoundException($"Không tìm thấy tin nhắn với Id '{request.Id}'.");
            }
            return _mapper.Map<MessageResponse>(entity);
        }

        public async Task<MessageListResponse> GetByFilterAsync(
            EntityFilter<Message> filter,
            EntitySort<Message> sort,
            EntityPage page)
        {
            var (items, totalCount) = await _messageRepository.GetByFilterAsync(filter, sort, page);
            var response = new MessageListResponse
            {
                Items = _mapper.Map<List<MessageResponse>>(items),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
            return response;
        }

        public async Task<MessageResponse> CreateAsync(CreateMessageRequest request)
        {
            var entity = _mapper.Map<Message>(request);
            entity.Id = Guid.NewGuid().ToString();
            entity.Timestamp = DateTime.UtcNow;
            entity.IsRead = false;

            await _messageRepository.AddAsync(entity);

            var savedEntity = await _messageRepository.GetByIdAsync(entity.Id);
            return _mapper.Map<MessageResponse>(savedEntity);
        }

        public async Task UpdateAsync(UpdateMessageRequest request)
        {
            var existing = await _messageRepository.GetByIdAsync(request.Id);
            if (existing == null)
            {
                throw new NotFoundException($"Không tìm thấy tin nhắn với Id '{request.Id}'.");
            }

            // In this project, UpdateMessageRequest only contains IsRead
            existing.IsRead = request.IsRead;
            
            await _messageRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(DeleteMessageRequest request)
        {
            var exists = await _messageRepository.ExistsAsync(request.Id);
            if (!exists)
            {
                throw new NotFoundException($"Không tìm thấy tin nhắn với Id '{request.Id}'.");
            }
            await _messageRepository.DeleteAsync(request.Id);
        }
    }
}
