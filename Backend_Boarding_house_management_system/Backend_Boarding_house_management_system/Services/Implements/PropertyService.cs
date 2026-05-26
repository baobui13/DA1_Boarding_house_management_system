using Backend_Boarding_house_management_system.DTOs.Property.Requests;
using Backend_Boarding_house_management_system.DTOs.Property.Responses;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class PropertyService : IPropertyService
    {
        private readonly AppDbContext _context;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAreaRepository _areaRepository;
        private readonly IMapper _mapper;

        public PropertyService(
            AppDbContext context,
            IPropertyRepository propertyRepository,
            IUserRepository userRepository,
            IAreaRepository areaRepository,
            IMapper mapper)
        {
            _context = context;
            _propertyRepository = propertyRepository;
            _userRepository = userRepository;
            _areaRepository = areaRepository;
            _mapper = mapper;
        }

        public async Task<PropertyResponse> GetPropertyByIdAsync(GetPropertyByIdRequest request)
        {
            var property = await _propertyRepository.GetByIdAsync(request.Id);
            if (property == null)
                throw new NotFoundException("Khong tim thay bat dong san.");

            return _mapper.Map<PropertyResponse>(property);
        }

        public async Task<PropertyDetailResponse> GetPropertyDetailByIdAsync(GetPropertyByIdRequest request)
        {
            var property = await _propertyRepository.GetByIdWithDetailsAsync(request.Id);
            if (property == null)
                throw new NotFoundException("Khong tim thay bat dong san.");

            return _mapper.Map<PropertyDetailResponse>(property);
        }

        public async Task<PropertyListResponse> GetModerationPropertiesAsync(GetModerationPropertiesRequest request)
        {
            var status = ParseModerationStatus(request.Status, ModerationStatusEnum.Pending);
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var query = _context.Properties
                .AsNoTracking()
                .Where(property => property.ModerationStatus == status)
                .OrderByDescending(property => property.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PropertyListResponse
            {
                Items = _mapper.Map<List<PropertyResponse>>(items),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PropertyListResponse> GetPropertiesByFilterAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page)
        {
            var (properties, totalCount) = await _propertyRepository.GetByFilterAsync(filter, sort, page);
            return new PropertyListResponse
            {
                Items = _mapper.Map<List<PropertyResponse>>(properties),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
        }

        public async Task<PropertyResponse> CreatePropertyAsync(CreatePropertyRequest request)
        {
            var landlord = await _userRepository.GetByIdAsync(request.LandlordId);
            if (landlord == null)
                throw new NotFoundException($"Khong tim thay landlord voi Id '{request.LandlordId}'.");

            if (!string.Equals(landlord.Role, "Landlord", StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("User duoc chon khong phai landlord.");

            Area? area = null;
            if (!string.IsNullOrWhiteSpace(request.AreaId))
            {
                area = await _areaRepository.GetByIdAsync(request.AreaId);
                if (area == null)
                    throw new NotFoundException($"Khong tim thay khu vuc voi Id '{request.AreaId}'.");

                if (!string.Equals(area.LandlordId, request.LandlordId, StringComparison.Ordinal))
                    throw new BadRequestException("Khu vuc khong thuoc landlord da chon.");
            }

            var property = _mapper.Map<Property>(request);
            property.Id = Guid.NewGuid().ToString();
            property.CreatedAt = DateTime.UtcNow;
            property.ModerationStatus = ParseModerationStatus(request.ModerationStatus, ModerationStatusEnum.Pending);
            property.AvailabilityStatus = ParseAvailabilityStatus(request.Status, AvailabilityStatusEnum.Available);
            property.UpdatedAt = DateTime.UtcNow;

            await _propertyRepository.AddAsync(property);

            if (area != null)
            {
                area.RoomCount += 1;
                await _areaRepository.UpdateAsync(area);
            }

            return _mapper.Map<PropertyResponse>(property);
        }

        public async Task<bool> UpdatePropertyAsync(UpdatePropertyRequest request)
        {
            var property = await _propertyRepository.GetByIdAsync(request.Id);
            if (property == null)
                throw new NotFoundException($"Khong tim thay bat dong san voi Id '{request.Id}'.");

            var requestedStatus = request.Status;
            var requestedModerationStatus = request.ModerationStatus;
            _mapper.Map(request, property);
            property.AvailabilityStatus = ParseAvailabilityStatus(requestedStatus, property.AvailabilityStatus);
            property.ModerationStatus = ParseModerationStatus(requestedModerationStatus, property.ModerationStatus);
            property.UpdatedAt = DateTime.UtcNow;
            await _propertyRepository.UpdateAsync(property);
            return true;
        }

        public async Task<bool> ApprovePropertyAsync(ApprovePropertyRequest request)
        {
            var property = await _propertyRepository.GetByIdAsync(request.PropertyId);
            if (property == null)
                throw new NotFoundException($"Khong tim thay bat dong san voi Id '{request.PropertyId}'.");

            if (property.ModerationStatus != ModerationStatusEnum.Pending)
                throw new BadRequestException("Chi co the duyet bat dong san dang trong trang Thai cho duyet.");

            property.ModerationStatus = ModerationStatusEnum.Approved;
            property.ApprovedAt = DateTime.UtcNow;
            property.UpdatedAt = DateTime.UtcNow;
            
            await _propertyRepository.UpdateAsync(property);
            return true;
        }

        public async Task<bool> RejectPropertyAsync(RejectPropertyRequest request)
        {
            var property = await _propertyRepository.GetByIdAsync(request.PropertyId);
            if (property == null)
                throw new NotFoundException($"Khong tim thay bat dong san voi Id '{request.PropertyId}'.");

            if (property.ModerationStatus != ModerationStatusEnum.Pending)
                throw new BadRequestException("Chi co the tu choi bat dong san dang trong trang Thai cho duyet.");

            property.ModerationStatus = ModerationStatusEnum.Rejected;
            property.RejectionReason = request.RejectionReason;
            property.RejectedAt = DateTime.UtcNow;
            property.UpdatedAt = DateTime.UtcNow;
            
            await _propertyRepository.UpdateAsync(property);
            return true;
        }

        public async Task<bool> UpdateAvailabilityStatusAsync(UpdateAvailabilityStatusRequest request)
        {
            var property = await _propertyRepository.GetByIdAsync(request.PropertyId);
            if (property == null)
                throw new NotFoundException($"Khong tim thay bat dong san voi Id '{request.PropertyId}'.");

            if (property.ModerationStatus != ModerationStatusEnum.Approved)
                throw new BadRequestException("Chi co the cap nhat trang thai kha dung cho bat dong san da duoc duyet.");

            property.AvailabilityStatus = request.AvailabilityStatus;
            property.UpdatedAt = DateTime.UtcNow;
            
            await _propertyRepository.UpdateAsync(property);
            return true;
        }

        public async Task<bool> DeletePropertyAsync(DeletePropertyRequest request)
        {
            var property = await _propertyRepository.GetByIdAsync(request.Id);
            if (property == null)
                throw new NotFoundException($"Khong tim thay bat dong san voi Id '{request.Id}'.");

            var blockers = await GetPropertyDeleteBlockersAsync(request.Id);
            if (blockers.Count > 0)
            {
                throw new ConflictException(
                    $"Khong the xoa bat dong san voi Id '{request.Id}' vi van con du lieu lien quan: {string.Join(", ", blockers)}.");
            }

            var areaId = property.AreaId;

            try
            {
                await _propertyRepository.DeleteAsync(request.Id);

                if (!string.IsNullOrWhiteSpace(areaId))
                {
                    var area = await _areaRepository.GetByIdAsync(areaId);
                    if (area != null)
                    {
                        area.RoomCount = Math.Max(0, area.RoomCount - 1);
                        await _areaRepository.UpdateAsync(area);
                    }
                }
            }
            catch (DbUpdateException)
            {
                throw new ConflictException(
                    $"Khong the xoa bat dong san voi Id '{request.Id}' vi van con du lieu lien quan trong he thong.");
            }

            return true;
        }

        private async Task<List<string>> GetPropertyDeleteBlockersAsync(string propertyId)
        {
            var blockers = new List<string>();

            if (await _context.Contracts.AnyAsync(x => x.PropertyId == propertyId))
                blockers.Add("hop dong");

            if (await _context.Appointments.AnyAsync(x => x.PropertyId == propertyId))
                blockers.Add("lich hen");

            if (await _context.Messages.AnyAsync(x => x.PropertyId == propertyId))
                blockers.Add("tin nhan");

            if (await _context.Ratings.AnyAsync(x => x.PropertyId == propertyId))
                blockers.Add("danh gia");

            if (await _context.Complaints.AnyAsync(x => x.RelatedType == ComplaintRelatedType.Property && x.RelatedId == propertyId && x.Status != ComplaintStatus.Resolved))
                blockers.Add("khiếu nại chưa giải quyết");

            return blockers;
        }

        private static ModerationStatusEnum ParseModerationStatus(string? value, ModerationStatusEnum fallback)
        {
            if (string.IsNullOrWhiteSpace(value)) return fallback;
            return Enum.TryParse<ModerationStatusEnum>(value, true, out var parsed)
                ? parsed
                : fallback;
        }

        private static AvailabilityStatusEnum ParseAvailabilityStatus(string? value, AvailabilityStatusEnum fallback)
        {
            if (string.IsNullOrWhiteSpace(value)) return fallback;

            if (Enum.TryParse<AvailabilityStatusEnum>(value, true, out var parsed))
            {
                return parsed;
            }

            return value.Trim().ToLowerInvariant() switch
            {
                "unavailable" => AvailabilityStatusEnum.Maintenance,
                "repairing" => AvailabilityStatusEnum.Maintenance,
                "nearexpiry" => AvailabilityStatusEnum.Maintenance,
                _ => fallback,
            };
        }
    }
}
