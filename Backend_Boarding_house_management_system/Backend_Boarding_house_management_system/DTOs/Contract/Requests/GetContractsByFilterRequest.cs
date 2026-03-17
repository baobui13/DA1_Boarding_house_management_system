using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.Contract.Requests
{
    public class GetContractsByFilterRequest : PagedRequest
    {
        public string? RoomId { get; set; }
        public string? TenantId { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
    }
}
