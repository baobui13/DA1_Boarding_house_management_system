using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.User.Requests
{
    public class GetUsersByFilterRequest : PagedRequest
    {
        public string? Role { get; set; }

        public string? FullName { get; set; }

        public string? Address { get; set; }

        public DateTime? CreatedAfter { get; set; }

        public DateTime? CreatedBefore { get; set; }
    }
}