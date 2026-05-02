using Backend_Boarding_house_management_system.DTOs.Complaint.Requests;
using Backend_Boarding_house_management_system.DTOs.Complaint.Responses;
using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Page;
using Plainquire.Sort;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IComplaintService
    {
        Task<ComplaintResponse> GetComplaintByIdAsync(GetComplaintByIdRequest request);
        Task<ComplaintDetailResponse> GetComplaintDetailByIdAsync(GetComplaintByIdRequest request);
        Task<ComplaintListResponse> GetComplaintsByFilterAsync(
            EntityFilter<Complaint> filter,
            EntitySort<Complaint> sort,
            EntityPage page);
        Task<ComplaintResponse> CreateComplaintAsync(CreateComplaintRequest request);
        Task<bool> UpdateComplaintAsync(UpdateComplaintRequest request);
        Task<bool> DeleteComplaintAsync(DeleteComplaintRequest request);
    }
}
