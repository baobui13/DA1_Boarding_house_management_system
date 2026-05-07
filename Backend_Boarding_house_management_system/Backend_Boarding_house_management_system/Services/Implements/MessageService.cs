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
        private readonly IUserRepository _userRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IMapper _mapper;

        public MessageService(
            IMessageRepository messageRepository,
            IUserRepository userRepository,
            IPropertyRepository propertyRepository,
            IContractRepository contractRepository,
            IMapper mapper)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _propertyRepository = propertyRepository;
            _contractRepository = contractRepository;
            _mapper = mapper;
        }

        public async Task<MessageResponse> GetByIdAsync(GetMessageByIdRequest request)
        {
            var entity = await _messageRepository.GetByIdAsync(request.Id);
            if (entity == null)
            {
                throw new NotFoundException($"Khong tim thay tin nhan voi Id '{request.Id}'.");
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
            if (await _userRepository.GetByIdAsync(request.SenderId) == null)
                throw new NotFoundException($"Khong tim thay nguoi gui voi Id '{request.SenderId}'.");

            if (await _userRepository.GetByIdAsync(request.ReceiverId) == null)
                throw new NotFoundException($"Khong tim thay nguoi nhan voi Id '{request.ReceiverId}'.");

            if (string.Equals(request.SenderId, request.ReceiverId, StringComparison.Ordinal))
                throw new BadRequestException("Nguoi gui va nguoi nhan khong duoc giong nhau.");

            Property? property = null;
            if (!string.IsNullOrWhiteSpace(request.PropertyId))
            {
                property = await _propertyRepository.GetByIdAsync(request.PropertyId);
                if (property == null)
                    throw new NotFoundException($"Khong tim thay phong voi Id '{request.PropertyId}'.");
            }

            if (!string.IsNullOrWhiteSpace(request.ContractId))
            {
                var contract = await _contractRepository.GetByIdAsync(request.ContractId);
                if (contract == null)
                    throw new NotFoundException($"Khong tim thay hop dong voi Id '{request.ContractId}'.");

                if (property != null && !string.Equals(contract.PropertyId, property.Id, StringComparison.Ordinal))
                    throw new BadRequestException("Hop dong khong thuoc phong da chon.");

                var contractLandlordId = contract.Property.LandlordId;
                var participants = new[] { contract.TenantId, contractLandlordId };
                if (!participants.Contains(request.SenderId) || !participants.Contains(request.ReceiverId))
                    throw new BadRequestException("Nguoi gui/nguoi nhan khong hop le voi hop dong da chon.");
            }

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
                throw new NotFoundException($"Khong tim thay tin nhan voi Id '{request.Id}'.");
            }

            existing.IsRead = request.IsRead;

            await _messageRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(DeleteMessageRequest request)
        {
            var exists = await _messageRepository.ExistsAsync(request.Id);
            if (!exists)
            {
                throw new NotFoundException($"Khong tim thay tin nhan voi Id '{request.Id}'.");
            }
            await _messageRepository.DeleteAsync(request.Id);
        }
    }
}
