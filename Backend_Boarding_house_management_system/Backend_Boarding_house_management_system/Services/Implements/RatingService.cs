using AutoMapper;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.DTOs.Rating.Requests;
using Backend_Boarding_house_management_system.DTOs.Rating.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Page;
using Plainquire.Sort;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public RatingService(
            IRatingRepository ratingRepository,
            IUserRepository userRepository,
            IPropertyRepository propertyRepository,
            AppDbContext context,
            IMapper mapper)
        {
            _ratingRepository = ratingRepository;
            _userRepository = userRepository;
            _propertyRepository = propertyRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<RatingResponse> GetRatingByIdAsync(GetRatingByIdRequest request)
        {
            var rating = await _ratingRepository.GetByIdAsync(request.Id);
            if (rating == null)
                throw new NotFoundException($"Khong tim thay danh gia voi Id '{request.Id}'.");

            return _mapper.Map<RatingResponse>(rating);
        }

        public async Task<RatingDetailResponse> GetRatingDetailByIdAsync(GetRatingByIdRequest request)
        {
            var rating = await _ratingRepository.GetByIdWithDetailsAsync(request.Id);
            if (rating == null)
                throw new NotFoundException($"Khong tim thay danh gia voi Id '{request.Id}'.");

            return _mapper.Map<RatingDetailResponse>(rating);
        }

        public async Task<RatingListResponse> GetRatingsByFilterAsync(
            EntityFilter<Rating> filter,
            EntitySort<Rating> sort,
            EntityPage page)
        {
            var (ratings, totalCount) = await _ratingRepository.GetByFilterAsync(filter, sort, page);
            return new RatingListResponse
            {
                Items = _mapper.Map<List<RatingResponse>>(ratings),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
        }

        public async Task<RatingResponse> CreateRatingAsync(CreateRatingRequest request)
        {
            var tenant = await _userRepository.GetByIdAsync(request.TenantId);
            if (tenant == null)
                throw new NotFoundException($"Khong tim thay tenant voi Id '{request.TenantId}'.");

            if (!string.Equals(tenant.Role, "Tenant", StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("User duoc chon khong phai tenant.");

            var property = await _propertyRepository.GetByIdAsync(request.PropertyId);
            if (property == null)
                throw new NotFoundException($"Khong tim thay bat dong san voi Id '{request.PropertyId}'.");

            if (await _ratingRepository.ExistsByTenantAndPropertyAsync(request.TenantId, request.PropertyId))
                throw new ConflictException("Tenant nay da danh gia bat dong san nay roi.");

            // Kiểm tra xem Tenant có hợp đồng thuê phòng này hay không để chặn đánh giá ảo
            var hasRented = await _context.Contracts.AnyAsync(c =>
                c.PropertyId == request.PropertyId &&
                c.TenantId == request.TenantId &&
                (c.Status == ContractStatus.Active || c.Status == ContractStatus.Expired || c.Status == ContractStatus.Terminated));
            if (!hasRented)
                throw new BadRequestException("Bạn chỉ có thể đánh giá phòng trọ mà bạn đang hoặc đã từng thuê.");

            var rating = _mapper.Map<Rating>(request);
            rating.Id = Guid.NewGuid().ToString();
            rating.CreatedAt = DateTime.UtcNow;

            await _ratingRepository.AddAsync(rating);

            // Cập nhật điểm uy tín ReputationScore của Landlord dựa trên thái độ đánh giá
            var landlord = await _userRepository.GetByIdAsync(property.LandlordId);
            if (landlord != null)
            {
                int change = rating.AIAttitude switch
                {
                    RatingAttitude.Positive => 1,
                    RatingAttitude.Negative => -1,
                    _ => 0
                };
                landlord.ReputationScore += change;
                await _userRepository.UpdateAsync(landlord);
            }

            return _mapper.Map<RatingResponse>(rating);
        }

        public async Task<bool> UpdateRatingAsync(UpdateRatingRequest request)
        {
            var rating = await _ratingRepository.GetByIdAsync(request.Id);
            if (rating == null)
                throw new NotFoundException($"Khong tim thay danh gia voi Id '{request.Id}'.");

            _mapper.Map(request, rating);
            await _ratingRepository.UpdateAsync(rating);
            return true;
        }

        public async Task<bool> DeleteRatingAsync(DeleteRatingRequest request)
        {
            if (!await _ratingRepository.ExistsAsync(request.Id))
                throw new NotFoundException($"Khong tim thay danh gia voi Id '{request.Id}'.");

            await _ratingRepository.DeleteAsync(request.Id);
            return true;
        }
    }
}
