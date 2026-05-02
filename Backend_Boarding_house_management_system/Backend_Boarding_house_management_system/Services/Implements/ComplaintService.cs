using AutoMapper;
using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.DTOs.Complaint.Requests;
using Backend_Boarding_house_management_system.DTOs.Complaint.Responses;
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
    public class ComplaintService : IComplaintService
    {
        private readonly IComplaintRepository _complaintRepository;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ComplaintService(
            IComplaintRepository complaintRepository,
            IUserRepository userRepository,
            AppDbContext context,
            IMapper mapper)
        {
            _complaintRepository = complaintRepository;
            _userRepository = userRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<ComplaintResponse> GetComplaintByIdAsync(GetComplaintByIdRequest request)
        {
            var complaint = await _complaintRepository.GetByIdAsync(request.Id);
            if (complaint == null)
                throw new NotFoundException($"Khong tim thay khieu nai voi Id '{request.Id}'.");

            return _mapper.Map<ComplaintResponse>(complaint);
        }

        public async Task<ComplaintDetailResponse> GetComplaintDetailByIdAsync(GetComplaintByIdRequest request)
        {
            var complaint = await _complaintRepository.GetByIdWithDetailsAsync(request.Id);
            if (complaint == null)
                throw new NotFoundException($"Khong tim thay khieu nai voi Id '{request.Id}'.");

            return _mapper.Map<ComplaintDetailResponse>(complaint);
        }

        public async Task<ComplaintListResponse> GetComplaintsByFilterAsync(
            EntityFilter<Complaint> filter,
            EntitySort<Complaint> sort,
            EntityPage page)
        {
            var (complaints, totalCount) = await _complaintRepository.GetByFilterAsync(filter, sort, page);
            return new ComplaintListResponse
            {
                Items = _mapper.Map<List<ComplaintResponse>>(complaints),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
        }

        public async Task<ComplaintResponse> CreateComplaintAsync(CreateComplaintRequest request)
        {
            if (await _userRepository.GetByIdAsync(request.CreatorId) == null)
                throw new NotFoundException($"Khong tim thay user voi Id '{request.CreatorId}'.");

            await ValidateRelatedEntityAsync(request.RelatedType, request.RelatedId);

            var complaint = _mapper.Map<Complaint>(request);
            complaint.Id = Guid.NewGuid().ToString();
            complaint.CreatedAt = DateTime.UtcNow;
            complaint.Status = "Pending";

            await _complaintRepository.AddAsync(complaint);
            return _mapper.Map<ComplaintResponse>(complaint);
        }

        public async Task<bool> UpdateComplaintAsync(UpdateComplaintRequest request)
        {
            var complaint = await _complaintRepository.GetByIdAsync(request.Id);
            if (complaint == null)
                throw new NotFoundException($"Khong tim thay khieu nai voi Id '{request.Id}'.");

            var relatedType = request.RelatedType ?? complaint.RelatedType;
            var relatedId = request.RelatedId ?? complaint.RelatedId;
            await ValidateRelatedEntityAsync(relatedType, relatedId);

            _mapper.Map(request, complaint);

            if (string.Equals(complaint.Status, "Resolved", StringComparison.OrdinalIgnoreCase) && complaint.ResolvedAt == null)
            {
                complaint.ResolvedAt = DateTime.UtcNow;
            }

            await _complaintRepository.UpdateAsync(complaint);
            return true;
        }

        public async Task<bool> DeleteComplaintAsync(DeleteComplaintRequest request)
        {
            if (!await _complaintRepository.ExistsAsync(request.Id))
                throw new NotFoundException($"Khong tim thay khieu nai voi Id '{request.Id}'.");

            await _complaintRepository.DeleteAsync(request.Id);
            return true;
        }

        private async Task ValidateRelatedEntityAsync(string relatedType, string relatedId)
        {
            if (string.IsNullOrWhiteSpace(relatedType))
                throw new BadRequestException("RelatedType khong duoc de trong.");

            if (string.IsNullOrWhiteSpace(relatedId))
                throw new BadRequestException("RelatedId khong duoc de trong.");

            var exists = relatedType.Trim().ToLowerInvariant() switch
            {
                "invoice" => await _context.Invoices.AnyAsync(x => x.Id == relatedId),
                "contract" => await _context.Contracts.AnyAsync(x => x.Id == relatedId),
                "property" => await _context.Properties.AnyAsync(x => x.Id == relatedId),
                _ => throw new BadRequestException("RelatedType phai la Invoice, Contract hoac Property.")
            };

            if (!exists)
                throw new NotFoundException($"Khong tim thay doi tuong lien quan voi RelatedType '{relatedType}' va RelatedId '{relatedId}'.");
        }
    }
}
